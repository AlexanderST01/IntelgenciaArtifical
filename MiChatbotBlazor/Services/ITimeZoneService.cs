namespace MiChatbotBlazor.Services
{
    public interface ITimeZoneService
    {
        DateTime GetLocalTime(DateTime utcDateTime);
        DateTime GetCurrentLocalTime();
        string FormatTime(DateTime utcDateTime, string format = "HH:mm");
        string FormatDateTime(DateTime utcDateTime, string format = "dd/MM/yyyy HH:mm");
        string FormatMessageTime(DateTime utcDateTime);
    }
}
