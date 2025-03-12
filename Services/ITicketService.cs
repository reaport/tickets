using System.Collections.Generic;
using TicketModule.Models;

namespace TicketModule.Services
{
    public interface ITicketService
    {
        BuyTicketResponse BuyTicket(BuyTicketRequest request);
        CancelTicketResponse CancelTicket(string ticketId, string passengerId);
        TicketStatusResponse GetTicketStatus(string ticketId);
        TicketDetailsResponse GetTicketDetails(string ticketId);
        IEnumerable<PassengerInfo> GetPassengersForFlight(string flightId);
    }
}
