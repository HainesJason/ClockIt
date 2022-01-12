using ClockIt.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClockIt.Services
{
    public class ExcelReporterService : IReporter
    {
        public string GenerateReport(List<BookingReportRecord> bookings)
        {
            if (bookings.Count > 0)
            {
                var now = DateTime.UtcNow;
                var filename = $"ClockItReport_{now.Year}-{now.Month}-{now.Day}-{now.Hour}{now.Minute}{now.Second}{now.Millisecond}.xlsx";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("My Bookings");
                    // Create the headers
                    PropertyInfo[] properties = bookings.First().GetType().GetProperties();
                    List<string> headerNames = properties.Select(prop => prop.Name).ToList();
                    for (int i = 0; i < headerNames.Count; i++)
                    {
                        ws.Cell(1, i + 1).Value = headerNames[i];
                        ws.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    var row = 2;
                    
                    foreach (var booking in bookings)
                    {
                        ws.Cell(row, 1).Value = booking.Day;
                        ws.Cell(row, 2).Value = booking.Month;
                        ws.Cell(row, 3).Value = booking.Year;
                        ws.Cell(row, 4).Value = booking.Category;
                        ws.Cell(row, 5).Value = booking.StartBooking;
                        ws.Cell(row, 6).Value = booking.EndBooking;
                        ws.Cell(row, 7).Value = booking.LengthOfTimeInSeconds;
                        ws.Cell(row, 8).Value = booking.UserFriendlyLengthOfTime;
                        row++;
                    }
                    wb.SaveAs(filePath);
                }
                return filePath;
            }
            return string.Empty;
        }
    }
}
