using System.Collections.Generic;

namespace TicketModule.Services
{
    public interface ICateringService
    {
        /// <summary>
        /// Получение списка доступных типов питания из модуля "Кейтеринг"
        /// </summary>
        List<string> GetMealTypes();
    }
}
