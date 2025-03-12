using System;

namespace TicketModule.Models
{
    public class Ticket
    {
        public string ?TicketId { get; set; }
        public string ?PassengerId { get; set; }
        public string ?FlightId { get; set; }
        public string ?SeatClass { get; set; } // economy или business
        public string ?MealType { get; set; }  // может быть null или "на данный рейс питание недоступно"
        public string ?Baggage { get; set; }   // "да" или "нет"
        public string ?Status { get; set; }    // "куплен", "возвращён", "зарегистрирован"
        public string ?Direction { get; set; }
        public DateTime DepartureTime { get; set; }
    }

    // Запросы для контроллеров
    public class BuyTicketRequest
    {
        public string ?PassengerId { get; set; }
        public string ?FlightId { get; set; }
        public string ?SeatClass { get; set; } // economy, business
        public string ?MealType { get; set; }  // выбранный тип питания
        public string ?Baggage { get; set; }   // "да" или "нет"
    }

    public class CancelTicketRequest
    {
        public string ?PassengerId { get; set; }
    }

    public class UpdateTicketStatusRequest
    {
        public string ?Status { get; set; }
    }

    // Ответы для покупок
    public class BuyTicketResponse
    {
        public string ?TicketId { get; set; }
        public string ?Direction { get; set; }
        public DateTime DepartureTime { get; set; }
        public string ?Status { get; set; }
    }

    public class CancelTicketResponse
    {
        public string ?TicketId { get; set; }
        public string ?Status { get; set; }
    }

    public class TicketStatusResponse
    {
        public string ?TicketId { get; set; }
        public string ?Status { get; set; }
    }

    public class TicketDetailsResponse
    {
        public string ?TicketId { get; set; }
        public string ?PassengerId { get; set; }
        public string ?FlightId { get; set; }
        public string ?Direction { get; set; }
        public DateTime DepartureTime { get; set; }
        public string ?SeatClass { get; set; }
        public string ?MealType { get; set; }
        public string ?Baggage { get; set; }
        public string ?Status { get; set; }
    }

    // Информация о пассажире для рейса
    public class PassengerInfo
    {
        public string ?PassengerId { get; set; }
        public string ?SeatClass { get; set; }
        public string ?MealType { get; set; }
        public string ?Baggage { get; set; }
    }
}
