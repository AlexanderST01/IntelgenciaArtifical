using System.ComponentModel.DataAnnotations;

namespace MiChatbotBlazor.Data.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string Title { get; set; } = "Nueva Conversación";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
