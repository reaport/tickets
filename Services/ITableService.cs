using TicketModule.Models;

namespace TicketModule.Services
{
    public interface ITableService
    {
        /// <summary>
        /// Получение информации о рейсе из модуля "Табло"
        /// </summary>
        FlightInfo GetFlightInfo(string flightId);

        /// <summary>
        /// Получение списка доступных рейсов для продажи билетов.
        /// </summary>
        IEnumerable<FlightInfo> GetAvailableFlights();
    }
}
