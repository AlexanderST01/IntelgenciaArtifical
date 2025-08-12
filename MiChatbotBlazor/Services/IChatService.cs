using MiChatbotBlazor.Data.Models;

namespace MiChatbotBlazor.Services
{
    public interface IChatService
    {
        Task<int> GetLastSessionIdAsync(string userId);
        Task<List<ChatSession>> GetChatsSessionsAsync(string userId);
        Task<List<ChatMessage>> GetMessagesAsync(int sessionId);
        Task<ChatMessage> AddMessageAsync(int sessionId, string content, string sender);
        Task<ChatSession> CreateSessionAsync(string userId, string title = "Nueva Conversación");
        Task<List<ChatSession>> GetUserSessionsAsync(string userId);
        Task<ChatSession?> GetSessionAsync(int sessionId);
        Task DeleteSessionAsync(int sessionId);
        Task UpdateSessionTitleAsync(int sessionId, string newTitle);
    }

}
