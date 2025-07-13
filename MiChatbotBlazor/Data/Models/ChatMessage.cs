using System.ComponentModel.DataAnnotations;

namespace MiChatbotBlazor.Data.Models;

public class ChatMessage
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Sender { get; set; } = string.Empty; // "user" o "bot"
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    
    public string? MessageType { get; set; } = "text"; // text, image, file
    public bool IsRead { get; set; } = false;
}
