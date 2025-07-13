using Microsoft.EntityFrameworkCore;
using MiChatbotBlazor.Data.Models;

namespace MiChatbotBlazor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar relaciones
        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId);

        // Índices para mejor rendimiento
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.Timestamp);

        modelBuilder.Entity<ChatSession>()
            .HasIndex(s => s.UserId);

        // Datos semilla
        modelBuilder.Entity<ChatSession>().HasData(
            new ChatSession 
            { 
                Id = 1, 
                UserId = "demo", 
                Title = "Conversación de Demo",
                CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<ChatMessage>().HasData(
            new ChatMessage 
            { 
                Id = 1, 
                Content = "¡Hola! Soy tu asistente virtual. ¿En qué puedo ayudarte hoy?",
                Sender = "bot",
                SessionId = 1,
                Timestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
