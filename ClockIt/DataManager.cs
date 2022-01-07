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
    }
}
