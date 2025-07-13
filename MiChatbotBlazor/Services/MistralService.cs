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
