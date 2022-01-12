using System;
using System.Collections.Generic;
using System.Text;

namespace ClockIt.Models
{
    public class BookingReportRecord
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Category { get; set; }
        public DateTime StartBooking { get; set; }
        public DateTime EndBooking { get; set; }
        public double LengthOfTimeInSeconds { get; set; }
        public string UserFriendlyLengthOfTime { get; set; }
    }
}
