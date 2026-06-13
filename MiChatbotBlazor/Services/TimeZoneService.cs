using System.Globalization;

namespace MiChatbotBlazor.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly CultureInfo _culture;

        public TimeZoneService(IConfiguration configuration)
        {
            var timeZoneName = configuration["TimeZone:DisplayTimeZone"] ?? "SA Western Standard Time";
            var cultureName = configuration["TimeZone:Culture"] ?? "es-DO";
            
            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            }
            catch
            {
                // Si no se encuentra la zona horaria, crear una personalizada para República Dominicana (UTC-4)
                _timeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Dominican Republic Time",
                    TimeSpan.FromHours(-4),
                    "Hora de República Dominicana",
                    "AST"
                );
            }
            
            _culture = new CultureInfo(cultureName);
        }

        public DateTime GetLocalTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
        }

        public DateTime GetCurrentLocalTime()
        {
            return GetLocalTime(DateTime.UtcNow);
        }

        public string FormatTime(DateTime utcDateTime, string format = "HH:mm")
        {
            var localTime = GetLocalTime(utcDateTime);
            return localTime.ToString(format, _culture);
        }

        public string FormatDateTime(DateTime utcDateTime, string format = "dd/MM/yyyy HH:mm")
        {
            var localTime = GetLocalTime(utcDateTime);
            return localTime.ToString(format, _culture);
        }

        public string FormatMessageTime(DateTime utcDateTime)
        {
            var localTime = GetLocalTime(utcDateTime);
            var now = GetCurrentLocalTime();
            
            // Si es hoy, mostrar solo la hora
            if (localTime.Date == now.Date)
            {
                return localTime.ToString("HH:mm", _culture);
            }
            // Si es ayer
            else if (localTime.Date == now.Date.AddDays(-1))
            {
                return $"Ayer {localTime:HH:mm}";
            }
            // Si es de esta semana (últimos 7 días)
            else if (localTime.Date >= now.Date.AddDays(-7))
            {
                return $"{localTime:dddd HH:mm}";
            }
            // Si es más antiguo, mostrar fecha completa
            else
            {
                return localTime.ToString("dd/MM/yyyy HH:mm", _culture);
            }
        }
    }
}
