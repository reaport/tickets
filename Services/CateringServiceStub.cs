using System.Collections.Generic;
using TicketModule.Log;

namespace TicketModule.Services
{
    public class CateringServiceStub : ICateringService
    {
        public List<string> GetMealTypes()
        {
            Logger.Log("CateringServiceStub", "INFO", "Получение типов питания (заглушка)");
            return new List<string> { "Standard", "Vegetarian", "Vegan", "Gluten-Free" };
        }
    }
}
