using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketModule.Models;
using TicketModule.Log;

namespace TicketModule.Services
{
    public class TableService : ITableService
    {
        private readonly HttpClient _httpClient;
        private readonly FlightSettings _settings;
        
        public TableService(HttpClient httpClient, IOptions<FlightSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }
        
        /// <summary>
        /// Получение информации о конкретном рейсе с использованием эндпоинта GET /tickets.
        /// Значения aircraftId, cityFrom, cityTo и seatClass берутся из настроек.
        /// </summary>
        public FlightInfo GetFlightInfo(string flightId)
        {
            Logger.Log("TableService", "INFO", $"Получение данных о рейсе {flightId}");
            
            // Получаем значения из настроек
            string aircraftId = _settings.AircraftId!;
            string cityFrom = _settings.CityFrom!;
            string cityTo = _settings.CityTo!;
            string seatClass = _settings.SeatClass!;
            
            // Проверяем, что все необходимые настройки заданы
            if (string.IsNullOrEmpty(aircraftId) || 
                string.IsNullOrEmpty(cityFrom) || 
                string.IsNullOrEmpty(cityTo) ||
                string.IsNullOrEmpty(seatClass))
            {
                throw new InvalidOperationException("Не заданы обязательные настройки для формирования запроса к Табло: AircraftId, CityFrom, CityTo, SeatClass");
            }
            
            // Формируем строку запроса с параметрами (значения экранируются)
            string query = $"?flightId={Uri.EscapeDataString(flightId)}" +
                           $"&aircraftId={Uri.EscapeDataString(aircraftId)}" +
                           $"&cityFrom={Uri.EscapeDataString(cityFrom)}" +
                           $"&cityTo={Uri.EscapeDataString(cityTo)}" +
                           $"&seatClass={Uri.EscapeDataString(seatClass)}";
            
            // Отправляем запрос к эндпоинту /tickets
            var response = _httpClient.GetAsync("/tickets" + query).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            
            // Десериализуем ответ в объект, соответствующий контракту
            var purchaseInfo = JsonSerializer.Deserialize<TicketPurchaseInfoResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (purchaseInfo == null)
                return null!;
            
            // Преобразуем полученную информацию в объект FlightInfo
            var flightInfo = new FlightInfo
            {
                FlightId = purchaseInfo.FlightId,
                Direction = $"{purchaseInfo.CityFrom} -> {purchaseInfo.CityTo}",
                DepartureTime = purchaseInfo.TakeoffDateTime,
                startRegisterTime = purchaseInfo.startRegisterTime,
                AvailableSeats = new Dictionary<string, int>()
            };
            
            if (purchaseInfo.AvailableSeats != null)
            {
                foreach (var seat in purchaseInfo.AvailableSeats)
                {
                    flightInfo.AvailableSeats[seat.SeatClass!] = seat.SeatCount;
                }
            }
            
            return flightInfo;
        }
        
        /// <summary>
        /// Получение списка доступных рейсов с использованием эндпоинта GET /tickets/available.
        /// </summary>
        public IEnumerable<FlightInfo> GetAvailableFlights()
        {
            Logger.Log("TableService", "INFO", "Получение списка доступных рейсов");
            var response = _httpClient.GetAsync("/tickets/available").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            
            // Десериализуем ответ — ожидается массив объектов, соответствующих контракту
            var purchaseInfos = JsonSerializer.Deserialize<List<TicketPurchaseInfoResponse>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            var flights = new List<FlightInfo>();
            if (purchaseInfos != null)
            {
                foreach (var purchaseInfo in purchaseInfos)
                {
                    var flightInfo = new FlightInfo
                    {
                        FlightId = purchaseInfo.FlightId,
                        Direction = $"{purchaseInfo.CityFrom} -> {purchaseInfo.CityTo}",
                        DepartureTime = purchaseInfo.TakeoffDateTime,
                        AvailableSeats = new Dictionary<string, int>()
                    };
                    
                    if (purchaseInfo.AvailableSeats != null)
                    {
                        foreach (var seat in purchaseInfo.AvailableSeats)
                        {
                            flightInfo.AvailableSeats[seat.SeatClass!] = seat.SeatCount;
                        }
                    }
                    flights.Add(flightInfo);
                }
            }
            return flights;
        }
    }
    
    // Классы для десериализации ответа согласно контракту Табло
    public class TicketPurchaseInfoResponse
    {
        public string? FlightId { get; set; }
        public string? AircraftId { get; set; }
        public string? CityFrom { get; set; }
        public string? CityTo { get; set; }
        public List<SeatInfo>? AvailableSeats { get; set; }
        public string? Baggage { get; set; }
        public DateTime TakeoffDateTime { get; set; }
        public DateTime LandingDateTime { get; set; }
        public DateTime startRegisterTime { get; set; }
    }
    
    public class SeatInfo
    {
        public string? SeatClass { get; set; }
        public int SeatCount { get; set; }
    }
}
