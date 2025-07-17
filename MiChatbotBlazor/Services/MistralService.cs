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
        // Convertir el historial simple a formato completo
        var fullHistory = conversationHistory.Select(msg => new { Content = msg, Sender = "user" }).ToList<dynamic>();
        return await GetResponseAsync(userMessage, fullHistory);
    }

    public async Task<string> GetResponseAsync(string userMessage, List<dynamic> conversationHistory)
    {
        // 1. Preparar knowledge base para el prompt
        var faqs = _kbService.GetAllFAQs();
        var faqsText = string.Join("\n", faqs.Select(f => $"P: {f.question}\nR: {f.answer}"));

        // Debug: Verificar que se están cargando las FAQs
        Console.WriteLine($"[DEBUG] FAQs cargadas: {faqs.Count}");
        Console.WriteLine($"[DEBUG] FAQ Text: {faqsText}");
        Console.WriteLine($"[DEBUG] User question: {userMessage}");
        Console.WriteLine($"[DEBUG] Conversation history count: {conversationHistory.Count}");

        // 2. Prompt más específico para Mistral
        var systemPrompt = $@"Eres un asistente virtual experto en inteligencia artificial.

IMPORTANTE: Tienes una base de conocimiento específica que DEBES PRIORIZAR. Si encuentras una coincidencia exacta o muy similar en estas preguntas, responde EXACTAMENTE con la respuesta proporcionada:

{faqsText}

INSTRUCCIONES:
1. Si la pregunta del usuario coincide exactamente con alguna de las preguntas de arriba, responde con la respuesta exacta.
2. Si la pregunta es muy similar (misma intención), usa la respuesta proporcionada.
3. Solo si NO hay coincidencia, responde como experto en IA.
4. Siempre responde en español.
5. Mantén el contexto de la conversación previa.

Analiza cuidadosamente la pregunta del usuario y busca coincidencias en tu base de conocimiento.";

        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Agregar historial de conversación completo (alternando entre user y assistant)
            foreach(var historyItem in conversationHistory.TakeLast(8)) // Últimos 8 mensajes
            {
                var role = historyItem.Sender == "user" ? "user" : "assistant";
                messages.Add(new { role = role, content = historyItem.Content });
                Console.WriteLine($"[DEBUG] Adding to history: {role} - {historyItem.Content}");
            }

            // Agregar mensaje actual
            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = _configuration["Mistral:Model"] ?? "mistral-small-latest",
                messages = messages,
                max_tokens = 500,
                temperature = 0.3, // Temperatura baja para respuestas más consistentes
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Debug: Mostrar el request que se envía
            Console.WriteLine($"[DEBUG] Request JSON: {json}");

            var response = await _httpClient.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Debug: Mostrar la respuesta completa
                Console.WriteLine($"[DEBUG] Response: {responseContent}");
                
                var mistralResponse = JsonSerializer.Deserialize<MistralResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var result = mistralResponse?.choices?.FirstOrDefault()?.message?.content ?? 
                       "Lo siento, no pude procesar tu mensaje.";
                
                Console.WriteLine($"[DEBUG] Final result: {result}");
                return result;
            }
            else
            {
                Console.WriteLine($"[DEBUG] HTTP Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            return "Lo siento, hubo un error al conectar con el servicio de IA.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en MistralService: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
