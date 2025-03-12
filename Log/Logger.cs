using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TicketModule.Log
{
    public static class Logger
    {
        private static readonly string LogFolder = "log";
        private static readonly string LogFile = Path.Combine(LogFolder, "logs.txt");
        private static readonly string AuditFile = Path.Combine(LogFolder, "audit.txt");

        static Logger()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
        }

        public static void Log(string source, string level, string message)
        {
            var logEntry = $"{DateTime.UtcNow} | {source} | {level} | {message}";
            File.AppendAllText(LogFile, logEntry + Environment.NewLine);
        }

        public static void LogAudit(string ticketId, string action)
        {
            var auditEntry = $"{DateTime.UtcNow} | {ticketId} | {action}";
            File.AppendAllText(AuditFile, auditEntry + Environment.NewLine);
        }

        public static List<string> GetLogs()
        {
            if (!File.Exists(LogFile))
                return new List<string>();
            return File.ReadAllLines(LogFile).ToList();
        }

        public static List<AuditEntry> GetAuditEntries()
        {
            var list = new List<AuditEntry>();
            if (!File.Exists(AuditFile))
                return list;
            var lines = File.ReadAllLines(AuditFile);
            foreach(var line in lines)
            {
                var parts = line.Split('|');
                if(parts.Length >= 3)
                {
                    list.Add(new AuditEntry 
                    { 
                        Timestamp = parts[0].Trim(), 
                        TicketId = parts[1].Trim(), 
                        Action = parts[2].Trim() 
                    });
                }
            }
            return list;
        }
    }

    public class AuditEntry
    {
        public string ?Timestamp { get; set; }
        public string ?TicketId { get; set; }
        public string ?Action { get; set; }
    }
}
