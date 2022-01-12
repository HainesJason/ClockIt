using ClockIt.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClockIt.Services
{
    public interface IReporter
    {
        string GenerateReport(List<BookingReportRecord> bookings);
    }
}
