using System.Collections.Generic;
using TicketModule.Log;

namespace TicketModule.ViewModels
{
    public class AdminViewModel
    {
        public int TotalSold { get; set; }
        public int TotalReturned { get; set; }
        public List<string> ?Logs { get; set; }
        public List<AuditEntry> ?Audit { get; set; }
    }
}
