using System.Collections.Concurrent;

namespace TicketModule.Data
{
    public class FlightRepository
    {
        // Хранит флаг регистрации для каждого рейса и данные о доступных местах (seatClass -> количество)
        private readonly ConcurrentDictionary<string, bool> _registrationStarted = new();
        private readonly ConcurrentDictionary<string, Dictionary<string, int>> _seatAvailability = new();

        // Метод для инициализации рейса (при первом обращении к Табло)
        public void InitializeFlight(string flightId, Dictionary<string, int> availableSeats)
        {
            _seatAvailability.TryAdd(flightId, availableSeats);
            _registrationStarted.TryAdd(flightId, false);
        }

        public bool IsRegistrationStarted(string flightId)
        {
            if (_registrationStarted.TryGetValue(flightId, out var started))
                return started;
            return false;
        }

        public void MarkRegistrationStarted(string flightId)
        {
            _registrationStarted[flightId] = true;
        }

        public bool TryReserveSeat(string flightId, string seatClass)
        {
            if (_seatAvailability.TryGetValue(flightId, out var seats))
            {
                if (seats.ContainsKey(seatClass) && seats[seatClass] > 0)
                {
                    seats[seatClass]--;
                    return true;
                }
            }
            return false;
        }

        public void ReleaseSeat(string flightId, string seatClass)
        {
            if (_seatAvailability.TryGetValue(flightId, out var seats))
            {
                if (seats.ContainsKey(seatClass))
                {
                    seats[seatClass]++;
                }
            }
        }

        public Dictionary<string, int> GetSeatAvailability(string flightId)
        {
            if (_seatAvailability.TryGetValue(flightId, out var seats))
                return seats;
            return new Dictionary<string, int>();
        }
    }
}
