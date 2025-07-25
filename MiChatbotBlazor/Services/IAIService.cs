namespace MiChatbotBlazor.Services
{
    public interface IAIService
    {
        Task<string> GetResponseAsync(string userMessage);
        Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory);
        Task<string> GetResponseAsync(string userMessage, List<dynamic> conversationHistory);
    }
}
