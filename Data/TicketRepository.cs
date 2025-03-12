using System.Collections.Concurrent;
using TicketModule.Models;

namespace TicketModule.Data
{
    public class TicketRepository
    {
        // Используем потокобезопасное хранилище
        private readonly ConcurrentDictionary<string, Ticket> _tickets = new();

        public IEnumerable<Ticket> Tickets => _tickets.Values;

        public void AddTicket(Ticket ticket)
        {
            _tickets.TryAdd(ticket.TicketId!, ticket);
        }

        public Ticket GetTicket(string ticketId)
        {
            _tickets.TryGetValue(ticketId, out var ticket);
            return ticket!;
        }

        public IEnumerable<Ticket> GetTicketsByFlight(string flightId)
        {
            return _tickets.Values.Where(t => t.FlightId == flightId && t.Status == "куплен");
        }

        public void UpdateTicket(Ticket ticket)
        {
            _tickets[ticket.TicketId!] = ticket;
        }
    }
}
