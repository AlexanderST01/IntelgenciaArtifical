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
        // 1. Preparar knowledge base para el prompt
        var faqs = _kbService.GetAllFAQs();
        var faqsText = string.Join("\n", faqs.Select(f => $"P: {f.question}\nR: {f.answer}"));

        // 2. Prompt instructivo para Mistral
        var systemPrompt =
            "Eres un asistente virtual experto en inteligencia artificial. " +
            "Tienes la siguiente base de conocimiento de preguntas frecuentes (FAQ):\n" +
            faqsText +
            "\nSi la pregunta del usuario coincide exactamente o es muy similar a alguna de las preguntas frecuentes, responde exactamente con la respuesta proporcionada. Si no, responde como experto en IA. Responde en español.";

        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Agregar historial de conversación (alternar entre user y assistant)
            foreach(var message in conversationHistory)
            {
                messages.Add(new { role = "user", content = message });
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
