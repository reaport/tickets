using System;
using System.Collections.Generic;
using TicketModule.Models;
using TicketModule.Log;

namespace TicketModule.Services
{
    public class TableServiceStub : ITableService
    {
         public FlightInfo GetFlightInfo(string flightId)
        {
            Logger.Log("TableServiceStub", "INFO", $"Получение данных о рейсе {flightId} (заглушка)");
            return new FlightInfo
            {
                FlightId = flightId,
                Direction = "Город А -> Город Б",
                DepartureTime = DateTime.UtcNow.AddHours(2),
                AvailableSeats = new Dictionary<string, int>
                {
                    { "economy", 50 },
                    { "business", 10 }
                }
            };
        }
        public IEnumerable<FlightInfo> GetAvailableFlights()
        {
            Logger.Log("TableServiceStub", "INFO", "Получение списка доступных рейсов (заглушка)");
            return new List<FlightInfo>
            {
                new FlightInfo
                {
                    FlightId = "FL001",
                    Direction = "Город А -> Город Б",
                    DepartureTime = DateTime.UtcNow.AddHours(2),
                    AvailableSeats = new Dictionary<string, int>
                    {
                        { "economy", 50 },
                        { "business", 10 }
                    }
                },
                new FlightInfo
                {
                    FlightId = "FL002",
                    Direction = "Город С -> Город Д",
                    DepartureTime = DateTime.UtcNow.AddHours(3),
                    AvailableSeats = new Dictionary<string, int>
                    {
                        { "economy", 60 },
                        { "business", 15 }
                    }
                }
            };
        }
    }
}
