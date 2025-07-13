# 🤖 Guía Completa: Cómo Crear un Chatbot Blazor Server desde Cero

## 📋 Tabla de Contenidos
1. [Introducción](#introducción)
2. [Requisitos Previos](#requisitos-previos)
3. [Configuración del Proyecto](#configuración-del-proyecto)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Configuración de Servicios](#configuración-de-servicios)
6. [Desarrollo de Componentes](#desarrollo-de-componentes)
7. [Servicios y API](#servicios-y-api)
8. [Funcionalidades Avanzadas](#funcionalidades-avanzadas)
9. [Estilos y UI](#estilos-y-ui)
10. [Despliegue](#despliegue)

---

## 🎯 Introducción

Esta guía te enseñará paso a paso cómo crear un chatbot moderno usando **Blazor Server**, similar al UCNE Chatbot pero con la potencia de C# y .NET. El proyecto incluye:

- ✅ Interfaz de chat responsiva con Bootstrap/Tailwind
- ✅ Integración con IA (OpenAI/Azure OpenAI)
- ✅ SignalR para tiempo real
- ✅ Persistencia con Entity Framework
- ✅ Autenticación y autorización
- ✅ Componentes reutilizables
- ✅ Gestión de estado reactiva

---

## 🛠️ Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

- **.NET 8 SDK** o superior
- **Visual Studio 2022** o **VS Code**
- **SQL Server** o **SQLite** (para desarrollo)
- **Git** (opcional)

### Verificar instalación:
```bash
dotnet --version
dotnet --list-sdks
```

---

## 🚀 Configuración del Proyecto

### Paso 1: Crear el proyecto Blazor Server

```bash
# Crear nuevo proyecto Blazor Server
dotnet new blazorserver -n MiChatbotBlazor

# Navegar al directorio
cd MiChatbotBlazor

# Restaurar paquetes
dotnet restore
```

### Paso 2: Instalar paquetes NuGet necesarios

```bash
# Entity Framework para persistencia
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# Para SQLite (desarrollo)
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# SignalR para tiempo real
dotnet add package Microsoft.AspNetCore.SignalR

# Cliente HTTP
dotnet add package System.Net.Http.Json

# Servicios de IA (elige uno)
# Para OpenAI:
dotnet add package OpenAI

# Para Mistral AI:
dotnet add package Mistral.SDK

# Alternativamente, usar cliente HTTP directo (sin SDK específico):
# dotnet add package System.Net.Http.Json

# Para JSON
dotnet add package System.Text.Json
```

---

## 📁 Estructura del Proyecto

Organiza tu proyecto con la siguiente estructura:

```
MiChatbotBlazor/
├── Components/
│   ├── Chat/
│   │   ├── ChatComponent.razor
│   │   ├── ChatMessage.razor
│   │   ├── ChatInput.razor
│   │   └── LoadingIndicator.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Shared/
│       ├── Loading.razor
│       └── ErrorBoundary.razor
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Models/
│   │   ├── ChatMessage.cs
│   │   ├── ChatSession.cs
│   │   └── User.cs
│   └── Migrations/
├── Services/
│   ├── IChatService.cs
│   ├── ChatService.cs
│   ├── IAIService.cs
│   ├── OpenAIService.cs
│   └── SignalRChatHub.cs
├── Pages/
│   ├── Chat.razor
│   ├── Team.razor
│   └── Error.razor
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── appsettings.json
├── Program.cs
└── MiChatbotBlazor.csproj
```

---

## 🗄️ Configuración de la Base de Datos

### 1. Modelos de Datos

**Data/Models/ChatMessage.cs:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace MiChatbotBlazor.Data.Models
{
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
}
```

**Data/Models/ChatSession.cs:**
```csharp
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
```

### 2. Contexto de Base de Datos

**Data/ApplicationDbContext.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using MiChatbotBlazor.Data.Models;

namespace MiChatbotBlazor.Data
{
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
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<ChatMessage>().HasData(
                new ChatMessage 
                { 
                    Id = 1, 
                    Content = "¡Hola! Soy tu asistente virtual. ¿En qué puedo ayudarte hoy?",
                    Sender = "bot",
                    SessionId = 1,
                    Timestamp = DateTime.UtcNow
                }
            );
        }
    }
}
```

---

## 🔧 Servicios y Lógica de Negocio

### 1. Servicio de IA

**Services/IAIService.cs:**
```csharp
namespace MiChatbotBlazor.Services
{
    public interface IAIService
    {
        Task<string> GetResponseAsync(string userMessage);
        Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory);
    }
}
```

**Services/OpenAIService.cs:**
```csharp
using System.Text.Json;
using MiChatbotBlazor.Services;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
        _apiUrl = _configuration["OpenAI:ApiUrl"] ?? "https://api.openai.com/v1/chat/completions";
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        return await GetResponseAsync(userMessage, new List<string>());
    }

    public async Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "Eres un asistente virtual útil y amigable. Responde de manera clara y concisa." }
            };

            // Agregar historial de conversación
            foreach (var historyMessage in conversationHistory.TakeLast(10)) // Limitar historial
            {
                messages.Add(new { role = "user", content = historyMessage });
            }

            // Agregar mensaje actual
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                max_tokens = 500,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                
                return openAIResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "Lo siento, no pude procesar tu mensaje.";
            }
            
            return "Lo siento, hubo un error al conectar con el servicio de IA.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en OpenAIService: {ex.Message}");
            return "Lo siento, ocurrió un error inesperado.";
        }
    }

    // Clases para deserialización
    private class OpenAIResponse
    {
        public Choice[]? choices { get; set; }
    }

    private class Choice
    {
        public Message? message { get; set; }
    }

    private class Message
    {
        public string? content { get; set; }
    }
}
```

### 2. Servicio de Mistral AI (Alternativa)

**Services/MistralService.cs:**
```csharp
using System.Text.Json;
using MiChatbotBlazor.Services;

public class MistralService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public MistralService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["Mistral:ApiKey"] ?? throw new ArgumentNullException("Mistral:ApiKey");
        _apiUrl = _configuration["Mistral:ApiUrl"] ?? "https://api.mistral.ai/v1/chat/completions";
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        return await GetResponseAsync(userMessage, new List<string>());
    }

    public async Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "Eres un asistente virtual útil y amigable. Responde de manera clara y concisa en español." }
            };

            // Agregar historial de conversación (alternar entre user y assistant)
            for (int i = 0; i < conversationHistory.Count && i < 10; i++) // Limitar historial
            {
                messages.Add(new { role = "user", content = conversationHistory[i] });
                // Aquí podrías agregar las respuestas anteriores del asistente si las tienes
            }

            // Agregar mensaje actual
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _configuration["Mistral:Model"] ?? "mistral-small-latest", // o "mistral-medium-latest"
                messages = messages,
                max_tokens = 500,
                temperature = 0.7,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mistralResponse = JsonSerializer.Deserialize<MistralResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                return mistralResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "Lo siento, no pude procesar tu mensaje.";
            }
            
            return "Lo siento, hubo un error al conectar con el servicio de IA.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en MistralService: {ex.Message}");
            return "Lo siento, ocurrió un error inesperado.";
        }
    }

    // Clases para deserialización de Mistral
    private class MistralResponse
    {
        public MistralChoice[]? choices { get; set; }
    }

    private class MistralChoice
    {
        public MistralMessage? message { get; set; }
    }

    private class MistralMessage
    {
        public string? content { get; set; }
    }
}
```

### 3. Servicio de IA Genérico (Usando HttpClient directo)

**Services/GenericAIService.cs:**
```csharp
using System.Text.Json;
using MiChatbotBlazor.Services;

public class GenericAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly string _model;
    private readonly string _provider;

    public GenericAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _provider = _configuration["AI:Provider"] ?? "openai"; // openai, mistral, anthropic, etc.
        
        switch (_provider.ToLower())
        {
            case "mistral":
                _apiKey = _configuration["Mistral:ApiKey"] ?? throw new ArgumentNullException("Mistral:ApiKey");
                _apiUrl = "https://api.mistral.ai/v1/chat/completions";
                _model = "mistral-small-latest";
                break;
            case "openai":
            default:
                _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
                _apiUrl = "https://api.openai.com/v1/chat/completions";
                _model = "gpt-3.5-turbo";
                break;
        }
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        return await GetResponseAsync(userMessage, new List<string>());
    }

    public async Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "Eres un asistente virtual útil y amigable. Responde de manera clara y concisa en español." }
            };

            // Agregar historial de conversación
            foreach (var historyMessage in conversationHistory.TakeLast(10))
            {
                messages.Add(new { role = "user", content = historyMessage });
            }

            // Agregar mensaje actual
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _model,
                messages = messages,
                max_tokens = 500,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonSerializer.Deserialize<GenericAIResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                return aiResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "Lo siento, no pude procesar tu mensaje.";
            }
            
            return $"Lo siento, hubo un error al conectar con {_provider}.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en {_provider}Service: {ex.Message}");
            return "Lo siento, ocurrió un error inesperado.";
        }
    }

    // Clases para deserialización genérica
    private class GenericAIResponse
    {
        public GenericChoice[]? choices { get; set; }
    }

    private class GenericChoice
    {
        public GenericMessage? message { get; set; }
    }

    private class GenericMessage
    {
        public string? content { get; set; }
    }
}
```

### 2. Servicio de Chat

**Services/IChatService.cs:**
```csharp
using MiChatbotBlazor.Data.Models;

namespace MiChatbotBlazor.Services
{
    public interface IChatService
    {
        Task<List<ChatMessage>> GetMessagesAsync(int sessionId);
        Task<ChatMessage> AddMessageAsync(int sessionId, string content, string sender);
        Task<ChatSession> CreateSessionAsync(string userId, string title = "Nueva Conversación");
        Task<List<ChatSession>> GetUserSessionsAsync(string userId);
        Task<ChatSession?> GetSessionAsync(int sessionId);
        Task DeleteSessionAsync(int sessionId);
    }
}
```

**Services/ChatService.cs:**
```csharp
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
```

---

## 🎨 Componentes Blazor

### 1. Componente Principal del Chat

**Components/Chat/ChatComponent.razor:**
```razor
@using MiChatbotBlazor.Data.Models
@using MiChatbotBlazor.Services
@inject IChatService ChatService
@inject IAIService AIService
@inject IJSRuntime JSRuntime
@implements IAsyncDisposable

<div class="chat-container d-flex flex-column h-100">
    <!-- Header -->
    <div class="chat-header bg-primary text-white p-3 d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center">
            <img src="/images/bot-icon.png" alt="Bot" class="me-2" style="width: 40px; height: 40px;" />
            <div>
                <h5 class="mb-0">Mi Chatbot</h5>
                <small class="@(IsOnline ? "text-success" : "text-warning")">
                    @(IsOnline ? "En línea" : "Desconectado")
                </small>
            </div>
        </div>
        <button class="btn btn-outline-light btn-sm" @onclick="ClearChat">
            <i class="fas fa-plus"></i> Nuevo Chat
        </button>
    </div>

    <!-- Área de mensajes -->
    <div class="messages-container flex-grow-1 overflow-auto p-3" @ref="messagesContainer">
        @if (messages.Any())
        {
            @foreach (var message in messages)
            {
                <ChatMessage Message="message" OnCopyMessage="CopyToClipboard" />
            }
        }
        
        @if (isLoading)
        {
            <LoadingIndicator />
        }
    </div>

    <!-- Input de mensaje -->
    <ChatInput OnSendMessage="SendMessage" IsLoading="isLoading" />
</div>

<style>
    .chat-container {
        height: calc(100vh - 100px);
        max-width: 800px;
        margin: 0 auto;
        border: 1px solid #dee2e6;
        border-radius: 8px;
        overflow: hidden;
    }

    .messages-container {
        background-color: #f8f9fa;
    }

    .messages-container::-webkit-scrollbar {
        width: 6px;
    }

    .messages-container::-webkit-scrollbar-track {
        background: #f1f1f1;
    }

    .messages-container::-webkit-scrollbar-thumb {
        background: #888;
        border-radius: 3px;
    }
</style>

@code {
    [Parameter] public int SessionId { get; set; } = 1;
    
    private List<ChatMessage> messages = new();
    private bool isLoading = false;
    private bool IsOnline => true; // Implementar lógica de conexión
    private ElementReference messagesContainer;

    protected override async Task OnInitializedAsync()
    {
        await LoadMessages();
    }

    private async Task LoadMessages()
    {
        messages = await ChatService.GetMessagesAsync(SessionId);
        StateHasChanged();
        await ScrollToBottom();
    }

    private async Task SendMessage(string messageContent)
    {
        if (string.IsNullOrWhiteSpace(messageContent) || isLoading)
            return;

        try
        {
            isLoading = true;
            StateHasChanged();

            // Agregar mensaje del usuario
            var userMessage = await ChatService.AddMessageAsync(SessionId, messageContent, "user");
            messages.Add(userMessage);
            StateHasChanged();
            await ScrollToBottom();

            // Obtener respuesta de la IA
            var conversationHistory = messages
                .Where(m => m.Sender == "user")
                .Select(m => m.Content)
                .ToList();

            var aiResponse = await AIService.GetResponseAsync(messageContent, conversationHistory);

            // Agregar respuesta del bot
            var botMessage = await ChatService.AddMessageAsync(SessionId, aiResponse, "bot");
            messages.Add(botMessage);
            
            StateHasChanged();
            await ScrollToBottom();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando mensaje: {ex.Message}");
            var errorMessage = await ChatService.AddMessageAsync(SessionId, 
                "Lo siento, hubo un error. Intenta de nuevo.", "bot");
            messages.Add(errorMessage);
            StateHasChanged();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task ClearChat()
    {
        var newSession = await ChatService.CreateSessionAsync("demo", "Nueva Conversación");
        SessionId = newSession.Id;
        await LoadMessages();
    }

    private async Task CopyToClipboard(string text)
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        // Mostrar notificación de copiado (implementar)
    }

    private async Task ScrollToBottom()
    {
        await Task.Delay(50); // Pequeño delay para asegurar que el DOM se actualice
        await JSRuntime.InvokeVoidAsync("scrollToBottom", messagesContainer);
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup si es necesario
    }
}
```

### 2. Componente de Mensaje Individual

**Components/Chat/ChatMessage.razor:**
```razor
@using MiChatbotBlazor.Data.Models

<div class="message-wrapper mb-3 @(Message.Sender == "user" ? "text-end" : "text-start")">
    <div class="d-flex @(Message.Sender == "user" ? "justify-content-end" : "justify-content-start")">
        <div class="message-content @GetMessageClasses() position-relative" style="max-width: 70%;">
            @if (Message.Sender == "bot")
            {
                <img src="/images/bot-avatar.png" alt="Bot" class="message-avatar me-2" />
            }
            
            <div class="message-bubble @GetBubbleClasses()">
                <p class="mb-1">@Message.Content</p>
                <small class="message-time text-muted">
                    @Message.Timestamp.ToString("HH:mm")
                </small>
                
                @if (Message.Sender == "bot")
                {
                    <button class="btn btn-sm btn-outline-secondary copy-btn" 
                            @onclick="() => OnCopyMessage.InvokeAsync(Message.Content)"
                            title="Copiar mensaje">
                        <i class="fas fa-copy"></i>
                    </button>
                }
            </div>
            
            @if (Message.Sender == "user")
            {
                <img src="/images/user-avatar.png" alt="Usuario" class="message-avatar ms-2" />
            }
        </div>
    </div>
</div>

<style>
    .message-avatar {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        object-fit: cover;
    }

    .message-bubble {
        padding: 12px 16px;
        border-radius: 18px;
        position: relative;
        word-wrap: break-word;
    }

    .message-bubble.user {
        background-color: #007bff;
        color: white;
    }

    .message-bubble.bot {
        background-color: #e9ecef;
        color: #333;
    }

    .copy-btn {
        position: absolute;
        top: 4px;
        right: 4px;
        padding: 4px 8px;
        opacity: 0;
        transition: opacity 0.2s;
    }

    .message-bubble:hover .copy-btn {
        opacity: 1;
    }

    .message-time {
        font-size: 0.75rem;
    }
</style>

@code {
    [Parameter] public ChatMessage Message { get; set; } = null!;
    [Parameter] public EventCallback<string> OnCopyMessage { get; set; }

    private string GetMessageClasses()
    {
        return Message.Sender == "user" ? "align-items-end" : "align-items-start";
    }

    private string GetBubbleClasses()
    {
        return Message.Sender == "user" ? "user" : "bot";
    }
}
```

### 3. Componente de Input

**Components/Chat/ChatInput.razor:**
```razor
<div class="chat-input-container bg-white border-top p-3">
    <div class="input-group">
        <input type="text" 
               class="form-control" 
               placeholder="Escribe tu mensaje aquí..."
               @bind="messageText"
               @onkeypress="HandleKeyPress"
               disabled="@IsLoading" />
        
        <button class="btn btn-primary" 
                @onclick="SendMessage"
                disabled="@(IsLoading || string.IsNullOrWhiteSpace(messageText))">
            @if (IsLoading)
            {
                <span class="spinner-border spinner-border-sm me-1" role="status"></span>
            }
            else
            {
                <i class="fas fa-paper-plane"></i>
            }
        </button>
    </div>
</div>

<style>
    .chat-input-container {
        border-top: 1px solid #dee2e6;
    }

    .input-group .form-control {
        border-radius: 20px 0 0 20px;
        border-right: none;
    }

    .input-group .btn {
        border-radius: 0 20px 20px 0;
        border-left: none;
    }

    .input-group .form-control:focus {
        box-shadow: none;
        border-color: #007bff;
    }
</style>

@code {
    [Parameter] public EventCallback<string> OnSendMessage { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private string messageText = string.Empty;

    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(messageText) && !IsLoading)
        {
            var message = messageText;
            messageText = string.Empty;
            await OnSendMessage.InvokeAsync(message);
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }
}
```

### 4. Indicador de Carga

**Components/Chat/LoadingIndicator.razor:**
```razor
<div class="loading-indicator d-flex justify-content-start mb-3">
    <div class="d-flex align-items-start">
        <img src="/images/bot-avatar.png" alt="Bot" class="message-avatar me-2" />
        <div class="loading-bubble bg-light p-3 rounded">
            <div class="typing-indicator">
                <span></span>
                <span></span>
                <span></span>
            </div>
        </div>
    </div>
</div>

<style>
    .message-avatar {
        width: 32px;
        height: 32px;
        border-radius: 50%;
    }

    .loading-bubble {
        border-radius: 18px;
    }

    .typing-indicator {
        display: flex;
        align-items: center;
        gap: 4px;
    }

    .typing-indicator span {
        height: 8px;
        width: 8px;
        background-color: #007bff;
        border-radius: 50%;
        display: inline-block;
        animation: typing 1.4s infinite ease-in-out;
    }

    .typing-indicator span:nth-child(1) {
        animation-delay: -0.32s;
    }

    .typing-indicator span:nth-child(2) {
        animation-delay: -0.16s;
    }

    @keyframes typing {
        0%, 80%, 100% {
            transform: scale(0.8);
            opacity: 0.5;
        }
        40% {
            transform: scale(1);
            opacity: 1;
        }
    }
</style>
```

---

## 📄 Páginas Principales

### 1. Página del Chat

**Pages/Chat.razor:**
```razor
@page "/chat"
@page "/chat/{sessionId:int}"
@using MiChatbotBlazor.Components.Chat

<PageTitle>Chat - Mi Chatbot</PageTitle>

<div class="container-fluid h-100">
    <div class="row h-100">
        <!-- Sidebar de sesiones (opcional) -->
        <div class="col-md-3 d-none d-md-block bg-light border-end">
            <ChatSessions OnSessionSelected="LoadSession" CurrentSessionId="currentSessionId" />
        </div>
        
        <!-- Área principal del chat -->
        <div class="col-md-9 col-12 p-0">
            <ChatComponent SessionId="currentSessionId" />
        </div>
    </div>
</div>

@code {
    [Parameter] public int? SessionId { get; set; }
    
    private int currentSessionId = 1;

    protected override void OnInitialized()
    {
        currentSessionId = SessionId ?? 1;
    }

    private void LoadSession(int sessionId)
    {
        currentSessionId = sessionId;
        StateHasChanged();
    }
}
```

---

## ⚙️ Configuración del Programa Principal

**Program.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using MiChatbotBlazor.Data;
using MiChatbotBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Servicios personalizados - Elige uno de los siguientes:

// Opción 1: Usar OpenAI
builder.Services.AddScoped<IAIService, OpenAIService>();

// Opción 2: Usar Mistral AI
// builder.Services.AddScoped<IAIService, MistralService>();

// Opción 3: Usar servicio genérico (configurable)
// builder.Services.AddScoped<IAIService, GenericAIService>();

builder.Services.AddScoped<IChatService, ChatService>();

// HttpClient para servicios externos
builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddHttpClient<MistralService>();
builder.Services.AddHttpClient<GenericAIService>();

// SignalR (para funcionalidades en tiempo real)
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Migrar base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        context.Database.EnsureCreated();
    }
    else
    {
        context.Database.Migrate();
    }
}

app.Run();
```

---

## 📝 Configuración de Archivos

### 1. appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=chatbot.db"
  },
  "AI": {
    "Provider": "openai"
  },
  "OpenAI": {
    "ApiKey": "tu-openai-api-key-aqui",
    "ApiUrl": "https://api.openai.com/v1/chat/completions",
    "Model": "gpt-3.5-turbo"
  },
  "Mistral": {
    "ApiKey": "tu-mistral-api-key-aqui",
    "ApiUrl": "https://api.mistral.ai/v1/chat/completions",
    "Model": "mistral-small-latest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Configuración específica para cada proveedor:

#### Para usar **OpenAI**:
- Cambia `"AI:Provider"` a `"openai"`
- Obtén tu API key desde: https://platform.openai.com/api-keys
- Modelos disponibles: `gpt-3.5-turbo`, `gpt-4`, `gpt-4-turbo`

#### Para usar **Mistral AI**:
- Cambia `"AI:Provider"` a `"mistral"`
- Obtén tu API key desde: https://console.mistral.ai/
- Modelos disponibles: `mistral-small-latest`, `mistral-medium-latest`, `mistral-large-latest`

#### Comando para instalar paquete de Mistral:
```bash
# Instalar SDK oficial de Mistral
dotnet add package Mistral.SDK

# O usar cliente HTTP genérico (recomendado para flexibilidad)
dotnet add package System.Net.Http.Json
```

### 2. Layout Principal

**Components/Layout/MainLayout.razor:**
```razor
@inherits LayoutView
@using MiChatbotBlazor.Components.Layout

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>

<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
```

---

## 🚀 Ejecutar el Proyecto

### Comandos de desarrollo:

```bash
# Restaurar dependencias
dotnet restore

# Crear migración inicial (si usas Code First)
dotnet ef migrations add InitialCreate

# Aplicar migraciones
dotnet ef database update

# Ejecutar en modo desarrollo
dotnet run

# Ejecutar con hot reload
dotnet watch run
```

### Construir para producción:

```bash
# Publicar la aplicación
dotnet publish -c Release -o ./publish

# Con configuración específica
dotnet publish -c Release -o ./publish --runtime win-x64 --self-contained false
```

---

## 🔧 JavaScript Helpers

**wwwroot/js/chat.js:**
```javascript
window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        console.log('Texto copiado al portapapeles');
        return true;
    } catch (err) {
        console.error('Error al copiar: ', err);
        return false;
    }
};

window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};
```

---

## 🎨 Estilos CSS Personalizados

**wwwroot/css/chat.css:**
```css
:root {
    --primary-color: #007bff;
    --secondary-color: #6c757d;
    --success-color: #28a745;
    --danger-color: #dc3545;
    --warning-color: #ffc107;
    --info-color: #17a2b8;
    --light-color: #f8f9fa;
    --dark-color: #343a40;
}

.chat-page {
    height: 100vh;
    overflow: hidden;
}

.messages-container {
    height: calc(100vh - 200px);
    overflow-y: auto;
    scroll-behavior: smooth;
}

.message-fade-in {
    animation: fadeIn 0.3s ease-in;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

.btn-send {
    border-radius: 50%;
    width: 50px;
    height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.chat-input {
    border-radius: 25px;
    border: 2px solid #e9ecef;
    padding: 12px 20px;
}

.chat-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
}

/* Responsive design */
@media (max-width: 768px) {
    .messages-container {
        height: calc(100vh - 150px);
    }
    
    .message-bubble {
        max-width: 85%;
    }
}

/* Dark mode support */
@media (prefers-color-scheme: dark) {
    :root {
        --bg-color: #1a1a1a;
        --text-color: #ffffff;
        --border-color: #333333;
    }
    
    .chat-container {
        background-color: var(--bg-color);
        color: var(--text-color);
    }
}
```

---

## 🚀 Despliegue

### 1. Azure App Service

```bash
# Publicar a Azure
az webapp deployment source config-zip --resource-group myResourceGroup --name myApp --src publish.zip
```

### 2. IIS

```xml
<!-- web.config -->
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\MiChatbotBlazor.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" />
  </system.webServer>
</configuration>
```

### 3. Docker

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MiChatbotBlazor.csproj", "."]
RUN dotnet restore "MiChatbotBlazor.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MiChatbotBlazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiChatbotBlazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiChatbotBlazor.dll"]
```

---

## 🔍 Testing

### Configurar Unit Tests:

```bash
# Crear proyecto de pruebas
dotnet new xunit -n MiChatbotBlazor.Tests
dotnet add MiChatbotBlazor.Tests reference MiChatbotBlazor

# Agregar paquetes de testing
dotnet add MiChatbotBlazor.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add MiChatbotBlazor.Tests package bunit
```

**Tests/ChatServiceTests.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using MiChatbotBlazor.Data;
using MiChatbotBlazor.Services;
using Xunit;

namespace MiChatbotBlazor.Tests
{
    public class ChatServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _chatService = new ChatService(_context);
        }

        [Fact]
        public async Task AddMessageAsync_ShouldAddMessage()
        {
            // Arrange
            var session = await _chatService.CreateSessionAsync("test-user");

            // Act
            var message = await _chatService.AddMessageAsync(session.Id, "Test message", "user");

            // Assert
            Assert.NotNull(message);
            Assert.Equal("Test message", message.Content);
            Assert.Equal("user", message.Sender);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

---

## 📚 Funcionalidades Avanzadas

### 1. SignalR para Tiempo Real

**Services/SignalRChatHub.cs:**
```csharp
using Microsoft.AspNetCore.SignalR;

namespace MiChatbotBlazor.Services
{
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
```

### 2. Autenticación

```csharp
// En Program.cs
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });
```

---

## 🤖 Proveedores de IA Disponibles

### Comparación de Proveedores

| Proveedor | Ventajas | Desventajas | Costo |
|-----------|----------|-------------|-------|
| **OpenAI** | - Muy buena calidad<br>- Amplia documentación<br>- Soporte para múltiples idiomas | - Más costoso<br>- Limitaciones de rate limiting | Alto |
| **Mistral AI** | - Modelos rápidos<br>- Enfoque en privacidad<br>- Precios competitivos<br>- Bueno en francés/español | - Comunidad más pequeña<br>- Menos funcionalidades | Medio |

### Configuración por Proveedor

#### 🔥 **OpenAI (GPT)**
```bash
# Instalar paquete
dotnet add package OpenAI

# En appsettings.json
"AI": { "Provider": "openai" }
"OpenAI": {
  "ApiKey": "sk-...",
  "Model": "gpt-3.5-turbo"  // o gpt-4
}
```

#### ⚡ **Mistral AI**
```bash
# Instalar paquete (opcional - se puede usar HttpClient)
dotnet add package Mistral.SDK

# En appsettings.json  
"AI": { "Provider": "mistral" }
"Mistral": {
  "ApiKey": "tu-mistral-key",
  "Model": "mistral-small-latest"  // o mistral-medium-latest
}
```

### Cómo obtener las API Keys:

#### OpenAI:
1. Ve a https://platform.openai.com/api-keys
2. Crea una cuenta y verifica tu email
3. Agrega un método de pago
4. Genera una nueva API key

#### Mistral AI:
1. Ve a https://console.mistral.ai/
2. Crea una cuenta 
3. Ve a "API Keys" en el dashboard
4. Genera una nueva API key

### Cambiar de Proveedor en Runtime:
```csharp
// En Program.cs, registrar múltiples servicios:
builder.Services.AddKeyedScoped<IAIService, OpenAIService>("openai");
builder.Services.AddKeyedScoped<IAIService, MistralService>("mistral");

// En el componente, inyectar específico:
[Inject] IServiceProvider ServiceProvider { get; set; }

private IAIService GetAIService(string provider)
{
    return ServiceProvider.GetKeyedService<IAIService>(provider);
}
```

---

## 🎉 ¡Felicidades!

Has creado exitosamente un chatbot moderno con Blazor Server. Este proyecto incluye:

✅ **Arquitectura robusta** con .NET 8  
✅ **Persistencia** con Entity Framework  
✅ **UI reactiva** con Blazor Server  
✅ **Integración con IA** (OpenAI)  
✅ **Tiempo real** con SignalR  
✅ **Responsive design** con Bootstrap  
✅ **Testing** configurado  
✅ **Preparado para producción**  

### Próximos pasos:
1. Personalizar el diseño según tu marca
2. Agregar autenticación de usuarios
3. Implementar funcionalidades avanzadas (archivos, voz)
4. Optimizar rendimiento
5. Desplegar en Azure/IIS

¡Disfruta construyendo tu chatbot con Blazor! 🚀
