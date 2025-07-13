using Microsoft.EntityFrameworkCore;
using MiChatbotBlazor.Data;
using MiChatbotBlazor.Data.Models;

namespace MiChatbotBlazor.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetLastSessionIdAsync(string userId)
        {
            var session = await _context.ChatSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();

            return session?.Id ?? 0;
        }   

        public async Task<List<ChatMessage>> GetMessagesAsync(int sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(int sessionId, string content, string sender)
        {
            var message = new ChatMessage
            {
                SessionId = sessionId,
                Content = content,
                Sender = sender,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            
            // Actualizar timestamp de la sesión
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<ChatSession> CreateSessionAsync(string userId, string title = "Nueva Conversación")
        {
            var session = new ChatSession
            {
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            // Agregar mensaje de bienvenida
            await AddMessageAsync(session.Id, 
                "¡Hola! Soy tu asistente virtual. ¿En qué puedo ayudarte hoy?", 
                "bot");

            return session;
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(string userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ChatSession?> GetSessionAsync(int sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
