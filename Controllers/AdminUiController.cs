using Microsoft.AspNetCore.Mvc;
using TicketModule.Data;
using TicketModule.Log;
using TicketModule.ViewModels;
using TicketModule.Models;
using System.Linq;

namespace TicketModule.Controllers
{
    // Путь для админки: http://localhost:8080/admin/ui
    [Route("admin/ui")]
    public class AdminUiController : Controller
    {
        private readonly TicketRepository _ticketRepository;
        public AdminUiController(TicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // GET /admin/ui
        [HttpGet("")]
        public IActionResult Index()
        {
            var model = new AdminViewModel
            {
                TotalSold = _ticketRepository.Tickets.Count(t => t.Status == "куплен"),
                TotalReturned = _ticketRepository.Tickets.Count(t => t.Status == "возвращён"),
                Logs = Logger.GetLogs(),
                Audit = Logger.GetAuditEntries()
            };

            return View(model);
        }

        // GET /admin/ui/updateStatus
        // Отображает форму для изменения статуса билета
        [HttpGet("updateStatus")]
        public IActionResult UpdateTicketStatusForm()
        {
            return View();
        }

        // POST /admin/ui/updateStatus
        [HttpPost("updateStatus")]
        public IActionResult UpdateTicketStatus(string ticketId, string status)
        {
            if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(status))
            {
                ModelState.AddModelError("", "TicketId и Status должны быть заполнены.");
                return View();
            }

            var ticket = _ticketRepository.GetTicket(ticketId);
            if (ticket == null)
            {
                ModelState.AddModelError("", "Билет не найден.");
                return View();
            }

            ticket.Status = status;
            _ticketRepository.UpdateTicket(ticket);

            // Логирование в аудит
            Logger.LogAudit(ticketId, $"Статус билета изменён на {status}");

            TempData["Message"] = $"Статус билета {ticketId} изменён на {status}.";
            return RedirectToAction("Index");
        }

        // POST /admin/ui/clearLogs — сброс логов и аудита
        [HttpPost("clearLogs")]
        public IActionResult ClearLogs()
        {
            // Удаляем файлы логов, если они существуют
            if (System.IO.File.Exists("log/logs.txt"))
            {
                System.IO.File.Delete("log/logs.txt");
            }
            if (System.IO.File.Exists("log/audit.txt"))
            {
                System.IO.File.Delete("log/audit.txt");
            }
            return RedirectToAction("Index");
        }
    }
}
