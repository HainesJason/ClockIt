using System;
using System.Collections.Generic;
using System.Text;

namespace ClockIt.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public double DifferenceInSeconds { get; set; }
    }
}
