using Microsoft.AspNetCore.Mvc;
using TicketModule.Models;
using TicketModule.Services;

namespace TicketModule.Controllers
{
    [ApiController]
    [Route("/")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ITableService _tableService; // Добавляем работу с Табло
        private readonly ICateringService _cateringService;
        
        public TicketsController(ITicketService ticketService, ITableService tableService, ICateringService cateringService)
        {
            _ticketService = ticketService;
            _tableService = tableService;
            _cateringService = cateringService;
        }

        /// <summary>
        /// Покупка билета или отображение списка доступных рейсов,
        /// если flightId не указан.
        /// </summary>
       [HttpPost("buy")]
        public IActionResult BuyTicket([FromBody] BuyTicketRequest request)
        {
            // Если flightId не передан, возвращаем список доступных рейсов
            if (string.IsNullOrEmpty(request.FlightId))
            {
                var flights = _tableService.GetAvailableFlights();
                if (flights == null || !flights.Any())
                    return NotFound(new { message = "Нет доступных рейсов" });
                return Ok(new
                {
                    message = "Выберите один из доступных рейсов перед покупкой",
                    availableFlights = flights
                });
            }

            // Если mealType не передан, возвращаем доступные варианты питания
            if (string.IsNullOrEmpty(request.MealType))
            {
                var mealOptions = _cateringService.GetMealTypes();
                if (mealOptions == null || !mealOptions.Any())
                    return NotFound(new { message = "Нет доступных типов питания" });
                return Ok(new
                {
                    message = "Выберите тип питания из доступных вариантов",
                    availableMealOptions = mealOptions
                });
            }

            try
            {
                var result = _ticketService.BuyTicket(request);
                return Ok(result);
            }
            catch (TicketException ex)
            {
                if (ex.ErrorCode == 400)
                    return BadRequest(new { message = ex.Message });
                if (ex.ErrorCode == 409)
                    return Conflict(new { message = ex.Message });
                return StatusCode(500, new { message = ex.Message });
            }
        }



        /// <summary>
        /// Возврат билета
        /// </summary>
        [HttpPost("cancel/{ticketId}")]
        public IActionResult CancelTicket(string ticketId, [FromBody] CancelTicketRequest request)
        {
            try
            {
                var result = _ticketService.CancelTicket(ticketId, request.PassengerId!);
                return Ok(result);
            }
            catch (TicketException ex)
            {
                if (ex.ErrorCode == 400)
                    return BadRequest(new { message = ex.Message });
                if (ex.ErrorCode == 404)
                    return NotFound(new { message = ex.Message });
                if (ex.ErrorCode == 409 || ex.ErrorCode == 422)
                    return Conflict(new { message = ex.Message });
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получение статуса билета
        /// </summary>
        [HttpGet("{ticketId}/status")]
        public IActionResult GetTicketStatus(string ticketId)
        {
            var result = _ticketService.GetTicketStatus(ticketId);
            if(result == null)
                return NotFound(new { message = "Билет не найден" });
            return Ok(result);
        }

        /// <summary>
        /// Получение деталей билета
        /// </summary>
        [HttpGet("{ticketId}/details")]
        public IActionResult GetTicketDetails(string ticketId)
        {
            var result = _ticketService.GetTicketDetails(ticketId);
            if(result == null)
                return NotFound(new { message = "Билет не найден" });
            return Ok(result);
        }

        /// <summary>
        /// Получение списка пассажиров для рейса (для модуля регистрации)
        /// При вызове данного метода для данного рейса прекращается возможность покупки/возврата билетов.
        /// </summary>
        [HttpGet("flight/{flightId}/passengers")]
        public IActionResult GetPassengersForFlight(string flightId)
        {
            var result = _ticketService.GetPassengersForFlight(flightId);
            if(result == null || !result.Any())
                return NotFound(new { message = "Пассажиры не найдены или рейс отсутствует" });
            return Ok(result);
        }
    }
}
