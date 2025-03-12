using System;
using System.Linq;
using System.Collections.Generic;
using TicketModule.Data;
using TicketModule.Log;
using TicketModule.Models;

namespace TicketModule.Services
{
    public class TicketService : ITicketService
    {
        private readonly TicketRepository _ticketRepository;
        private readonly FlightRepository _flightRepository;
        private readonly ITableService _tableService;
        private readonly ICateringService _cateringService;

        public TicketService(TicketRepository ticketRepository, FlightRepository flightRepository,
                             ITableService tableService, ICateringService cateringService)
        {
            _ticketRepository = ticketRepository;
            _flightRepository = flightRepository;
            _tableService = tableService;
            _cateringService = cateringService;
        }

        public BuyTicketResponse BuyTicket(BuyTicketRequest request)
        {
            // Валидация входных данных
            if (string.IsNullOrEmpty(request.PassengerId) ||
                string.IsNullOrEmpty(request.FlightId) ||
                string.IsNullOrEmpty(request.SeatClass) ||
                string.IsNullOrEmpty(request.Baggage))
            {
                throw new TicketException("Некорректные входные данные: не все обязательные поля заполнены.", 400);
            }

            if (request.Baggage != "да" && request.Baggage != "нет")
            {
                throw new TicketException("Некорректное значение для багажа. Допустимо только 'да' или 'нет'.", 400);
            }

            // Проверяем доступность Табло: если сервис не отвечает, покупка блокируется.
            IEnumerable<FlightInfo> availableFlights;
            try
            {
                availableFlights = _tableService.GetAvailableFlights();
            }
            catch (Exception ex)
            {
                Logger.Log("TicketService", "ERROR", $"Ошибка при обращении к Табло: {ex.Message}");
                throw new TicketException("Табло недоступно. Покупка билета невозможна.", 500);
            }
            if (availableFlights == null || !availableFlights.Any())
            {
                throw new TicketException("Нет доступных рейсов для продажи.", 500);
            }

            // Проверяем, есть ли выбранный рейс в списке, полученном от Табло
            var flight = availableFlights.FirstOrDefault(f => f.FlightId == request.FlightId);
            if (flight == null)
            {
                throw new TicketException("Выбранный рейс отсутствует в списке доступных. Покупка невозможна.", 400);
            }

            // Инициализируем данные о доступных местах для данного рейса
            _flightRepository.InitializeFlight(request.FlightId, flight.AvailableSeats!);

            // Проверяем наличие свободного места для указанного класса обслуживания
            if (!_flightRepository.TryReserveSeat(request.FlightId, request.SeatClass))
            {
                throw new TicketException("Нет доступных мест для выбранного класса обслуживания.", 409);
            }

            // Получаем список типов питания от Кейтеринга
            var mealTypes = _cateringService.GetMealTypes();
            if (mealTypes == null)
            {
                throw new TicketException("Не удалось получить список типов питания. Покупка невозможна.", 500);
            }

            // Если тип питания указан, проверяем его валидность
            if (!string.IsNullOrEmpty(request.MealType) && !mealTypes.Contains(request.MealType))
            {
                throw new TicketException($"Выбранный тип питания '{request.MealType}' недоступен на данном рейсе.", 400);
            }

            // Проверяем, что пассажир еще не купил билет на этот рейс
            var existingTicket = _ticketRepository.Tickets
                .FirstOrDefault(t => t.FlightId == request.FlightId &&
                                     t.PassengerId == request.PassengerId &&
                                     t.Status == "куплен");
            if (existingTicket != null)
            {
                throw new TicketException($"У вас уже есть билет на этот рейс (ID билета: {existingTicket.TicketId}).", 409);
            }

            // Если регистрация на рейс уже началась, покупка невозможна
            if (_flightRepository.IsRegistrationStarted(request.FlightId))
            {
                throw new TicketException("Регистрация на этот рейс уже началась. Покупка билета невозможна.", 409);
            }

            // Создаем новый билет
            var newTicket = new Ticket
            {
                TicketId = Guid.NewGuid().ToString(),
                PassengerId = request.PassengerId,
                FlightId = request.FlightId,
                SeatClass = request.SeatClass,
                MealType = request.MealType,
                Baggage = request.Baggage,
                Status = "куплен",
                Direction = flight.Direction,
                DepartureTime = flight.DepartureTime
            };

            _ticketRepository.AddTicket(newTicket);
            Logger.Log("TicketService", "INFO", $"Билет {newTicket.TicketId} успешно куплен.");

            return new BuyTicketResponse
            {
                TicketId = newTicket.TicketId,
                Direction = newTicket.Direction,
                DepartureTime = newTicket.DepartureTime,
                Status = newTicket.Status
            };
        }

        public CancelTicketResponse CancelTicket(string ticketId, string passengerId)
        {
            if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(passengerId))
                throw new TicketException("Некорректные параметры запроса", 400);

            var ticket = _ticketRepository.GetTicket(ticketId);
            if (ticket == null || ticket.PassengerId != passengerId)
                throw new TicketException("Билет не найден или не принадлежит данному пассажиру", 404);

            // Если регистрация на рейс уже началась, возврат невозможен
            if (_flightRepository.IsRegistrationStarted(ticket.FlightId!))
            {
                throw new TicketException("Возврат невозможен, так как регистрация уже началась", 409);
            }

            // Если билет уже возвращён, возврат невозможен
            if (ticket.Status == "возвращён")
            {
                throw new TicketException("Билет уже возвращён", 422);
            }

            // Обработка возврата: помечаем билет как возвращён и освобождаем место
            ticket.Status = "возвращён";
            _ticketRepository.UpdateTicket(ticket);
            _flightRepository.ReleaseSeat(ticket.FlightId!, ticket.SeatClass!);

            Logger.Log("TicketService", "INFO", $"Билет {ticket.TicketId} успешно возвращён.");

            return new CancelTicketResponse
            {
                TicketId = ticket.TicketId,
                Status = ticket.Status
            };
        }

        public TicketStatusResponse GetTicketStatus(string ticketId)
        {
            var ticket = _ticketRepository.GetTicket(ticketId);
            if (ticket == null)
                return null!;
            return new TicketStatusResponse
            {
                TicketId = ticket.TicketId,
                Status = ticket.Status
            };
        }

        public TicketDetailsResponse GetTicketDetails(string ticketId)
        {
            var ticket = _ticketRepository.GetTicket(ticketId);
            if (ticket == null)
                return null!;
            return new TicketDetailsResponse
            {
                TicketId = ticket.TicketId,
                PassengerId = ticket.PassengerId,
                FlightId = ticket.FlightId,
                Direction = ticket.Direction,
                DepartureTime = ticket.DepartureTime,
                SeatClass = ticket.SeatClass,
                MealType = ticket.MealType,
                Baggage = ticket.Baggage,
                Status = ticket.Status
            };
        }

        public IEnumerable<PassengerInfo> GetPassengersForFlight(string flightId)
        {
            // При получении списка пассажиров для рейса регистрация помечается как начатая,
            // что блокирует дальнейшую покупку/возврат билетов для данного рейса.
            _flightRepository.MarkRegistrationStarted(flightId);
            var tickets = _ticketRepository.GetTicketsByFlight(flightId);
            return tickets.Select(t => new PassengerInfo
            {
                PassengerId = t.PassengerId,
                SeatClass = t.SeatClass,
                MealType = t.MealType,
                Baggage = t.Baggage
            });
        }
    }

    // Кастомное исключение для модуля билетов
    public class TicketException : Exception
    {
        public int ErrorCode { get; }
        public TicketException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
