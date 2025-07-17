using System.Text.Json;
using MiChatbotBlazor.Services;

public class MistralService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly KnowledgeBaseService _kbService;

    public MistralService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["Mistral:ApiKey"] ?? throw new ArgumentNullException("Mistral:ApiKey");
        _apiUrl = _configuration["Mistral:ApiUrl"] ?? "https://api.mistral.ai/v1/chat/completions";
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        // Inicializa la base de conocimiento
        var kbPath = Path.Combine(AppContext.BaseDirectory, "Data", "knowledge_base.json");
        _kbService = new KnowledgeBaseService(kbPath);
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        return await GetResponseAsync(userMessage, new List<string>());
    }

    public async Task<string> GetResponseAsync(string userMessage, List<string> conversationHistory)
    {
        // 1. Buscar en la base de conocimiento
        var faqAnswer = _kbService.GetAnswer(userMessage);
        if (!string.IsNullOrEmpty(faqAnswer))
            return faqAnswer;

        // 2. Filtrar temas fuera de IA
        if (!_kbService.IsAboutAI(userMessage))
            return "Lo siento, solo puedo responder preguntas relacionadas con inteligencia artificial.";

        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "Eres un asistente virtual experto en inteligencia artificial. Solo responde preguntas sobre IA. Si la pregunta no es relevante, indica que solo puedes hablar de IA. Responde en español." }
            };

            // Agregar historial de conversación (alternar entre user y assistant)
            for (int i = 0; i < conversationHistory.Count && i < 10; i++)
            {
                messages.Add(new { role = "user", content = conversationHistory[i] });
            }

            // Agregar mensaje actual
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _configuration["Mistral:Model"] ?? "mistral-small-latest",
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
