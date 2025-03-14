using System;
using System.Collections.Generic;

namespace TicketModule.Models
{
    public class FlightInfo
    {
        public string ?FlightId { get; set; }
        public string ?Direction { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime startRegisterTime { get; set; }
        // Доступные места по классам (economy, business)
        public Dictionary<string, int> ?AvailableSeats { get; set; }
    }
}
