using ClockIt.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockIt
{
    public class DataManager
    {

        //private string _cnn = @"Data Source=./clockIt.db;";
        private string _cnn = $"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\ClockIt\\clockIt.db";
        //private string _cnn = @"Data Source=C:\ProgramData\ClockIt\clockIt.db";


        public async Task<int> StartRecording(StartClockModel booking)
        {
            using (IDbConnection cnn = new SqliteConnection(_cnn))
            {
                await cnn.ExecuteScalarAsync("insert into Booking " +
                                   "           (Category, Start)" +
                                   "        values" +
                                   "           (@Category, @Start)", booking);

                var id = await cnn.QueryAsync<int>("SELECT Id FROM booking WHERE End is null");
                return id.SingleOrDefault();
                //return await cnn.ExecuteScalarAsync("select last_insert_rowid()");

            }
        }

        public async Task<bool> PurgeBookings()
        {
            try
            {
                using (IDbConnection cnn = new SqliteConnection(_cnn))
                {
                    await cnn.ExecuteAsync("delete from booking");
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<int> StopRecording(StopClockModel booking)
        {
            var bookingRecord = await this.GetBookingById(booking.Id);
            var endDate = Convert.ToDateTime(booking.End);
            var startDate = Convert.ToDateTime(bookingRecord.Start);
            TimeSpan difference = endDate.Subtract(startDate);
            booking.DifferenceInSeconds = difference.TotalSeconds;

            using (IDbConnection cnn = new SqliteConnection(_cnn))
            {
                await cnn.ExecuteAsync("update booking set end = @End, " +
                    "DifferenceInSeconds = @DifferenceInSeconds" +
                    " where id = @Id ", booking);
                return 0;
            }
        }

        public async Task<Booking> GetBookingById(int id)
        {
            using (IDbConnection cnn = new SqliteConnection(_cnn))
            {
                var parameters = new { Id = id };
                var sql = "select * from Booking where Id = @Id";
                var output = await cnn.QueryAsync<Booking>(sql, parameters);
                return output.FirstOrDefault();
            }

        }

        public async Task<Booking> GetOpenBooking()
        {
            using (IDbConnection cnn = new SqliteConnection(_cnn))
            {

                var sql = "select * from Booking where End is null";
                var output = await cnn.QueryAsync<Booking>(sql);
                return output.FirstOrDefault();
            }
        }

        public async Task<List<Booking>> GetBookings(DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
            {
                start = DateTime.MinValue;
            }
            if (!end.HasValue)
            {
                end = DateTime.MaxValue;
            }

            using (IDbConnection cnn = new SqliteConnection(_cnn))
            {
                var parameters = new { Start = start.Value, End = end };
                var sql = "select * from Booking where Start >= @Start and Start <= @End";
                var output = await cnn.QueryAsync<Booking>(sql, parameters);
                return output.ToList();
            }
        }

        public async Task<List<BookingReportRecord>> GetBookingsForReport(DateTime? start, DateTime? end)
        {
            var bookings = await this.GetBookings(start, end);
            var completedBookings = bookings.Where(x => x.End != null).ToList();
            var reportData = (from c in completedBookings
                              select new BookingReportRecord
                              {
                                  Day = Convert.ToDateTime(c.Start).Day,
                                  Month = Convert.ToDateTime(c.Start).Month,
                                  Year = Convert.ToDateTime(c.Start).Year,
                                  Category = c.Category,
                                  StartBooking = Convert.ToDateTime(c.Start),
                                  EndBooking = Convert.ToDateTime(c.End),
                                  LengthOfTimeInSeconds = c.DifferenceInSeconds,
                                  UserFriendlyLengthOfTime = this.UserFriendlyTime(c.DifferenceInSeconds)
                              }).ToList();
            return reportData;
        }

        //TODO:  Move this into a static method as used in more than one place.
        private string UserFriendlyTime(double differenceInSeconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(differenceInSeconds);

            string secondsToConvert = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            return secondsToConvert;
        }


    }
}
