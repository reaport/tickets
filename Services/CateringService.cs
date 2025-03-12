using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using TicketModule.Log;

namespace TicketModule.Services
{
    public class CateringService : ICateringService
    {
        private readonly HttpClient _httpClient;

        public CateringService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<string> GetMealTypes()
        {
            Logger.Log("CateringService", "INFO", "Получение типов питания");
            // Выполняем GET запрос к /mealtypes
            var response = _httpClient.GetAsync("/mealtypes").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            // Ответ имеет структуру: { "mealTypes": [ "Standard", "Vegetarian", "Vegan", "Gluten-Free" ] }
            var result = JsonSerializer.Deserialize<MealTypesResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.MealTypes ?? new List<string>();
        }
    }

    public class MealTypesResponse
    {
        public List<string> ?MealTypes { get; set; }
    }
}
