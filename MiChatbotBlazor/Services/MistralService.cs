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
        // 1. Verificar primero si la pregunta está relacionada con IA
        if (!IsAIRelatedQuestion(userMessage))
        {
            return "Lo siento, solo puedo responder preguntas relacionadas con inteligencia artificial. ¿Tienes alguna pregunta sobre IA, machine learning, redes neuronales, o temas similares?";
        }

        // 2. Preparar knowledge base para el prompt
        var faqs = _kbService.GetAllFAQs();
        var faqsText = string.Join("\n", faqs.Select(f => $"P: {f.question}\nR: {f.answer}"));

        // Debug: Verificar que se están cargando las FAQs
        Console.WriteLine($"[DEBUG] FAQs cargadas: {faqs.Count}");
        Console.WriteLine($"[DEBUG] FAQ Text: {faqsText}");
        Console.WriteLine($"[DEBUG] User question: {userMessage}");
        Console.WriteLine($"[DEBUG] Conversation history count: {conversationHistory.Count}");

        // 3. Prompt más específico para Mistral
        var systemPrompt = $@"Eres un asistente virtual EXCLUSIVAMENTE especializado en inteligencia artificial.

RESTRICCIÓN IMPORTANTE: SOLO puedes responder preguntas sobre inteligencia artificial, machine learning, redes neuronales, algoritmos de IA, aplicaciones de IA, y temas directamente relacionados.

TIENES UNA BASE DE CONOCIMIENTO ESPECÍFICA que DEBES PRIORIZAR:

{faqsText}

INSTRUCCIONES ESTRICTAS:
1. Si la pregunta coincide exactamente con alguna de las preguntas de arriba, responde EXACTAMENTE con la respuesta proporcionada.
2. Si la pregunta es muy similar (misma intención sobre IA), usa la respuesta proporcionada como base.
3. Solo si NO hay coincidencia PERO la pregunta SÍ es sobre IA, responde como experto en IA.
4. Si la pregunta NO es sobre inteligencia artificial, responde con el mensaje de restricción.
5. Siempre responde en español.
6. Mantén el contexto de la conversación previa SOLO si es relevante para IA.

Temas que SÍ puedes responder: IA, machine learning, deep learning, redes neuronales, algoritmos, modelos predictivos, procesamiento de lenguaje natural, visión por computadora, robótica con IA, chatbots, automatización inteligente, ética en IA.

Temas que NO puedes responder: política, deportes, cocina, viajes, música no relacionada con IA, historia no tecnológica, salud general, finanzas personales, entretenimiento, etc.

Analiza cuidadosamente si la pregunta está relacionada con inteligencia artificial.";

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

    private bool IsAIRelatedQuestion(string userMessage)
    {
        // Convertir a minúsculas para comparación
        var message = userMessage.ToLower();
        
        // Primero verificar palabras que definitivamente NO son de IA
        var nonAIKeywords = new[]
        {
            // Temas generales que no son IA
            "cocina", "receta", "comida", "cocinar", "ingrediente",
            "deporte", "futbol", "fútbol", "basketball", "tenis", "natacion", "natación",
            "clima", "tiempo", "lluvia", "sol", "temperatura", "meteorologia", "meteorología",
            "musica", "música", "cancion", "canción", "banda", "artista", "concierto",
            "pelicula", "película", "actor", "actriz", "cine", "serie", "television", "televisión",
            "politica", "política", "gobierno", "presidente", "eleccion", "elección", "partido politico",
            "religion", "religión", "dios", "iglesia", "biblia", "rezar", "orar",
            "chiste", "broma", "humor", "gracioso", "risa", "comico", "cómico",
            "saludo", "hola", "buenos dias", "buenas noches", "como estas", "cómo estás",
            "amor", "relacion", "relación", "pareja", "cita", "noviazgo", "matrimonio",
            "viaje", "turismo", "hotel", "avion", "avión", "vacacion", "vacación", "playa",
            "dinero", "banco", "prestamo", "préstamo", "inversion", "inversión", "bolsa", "acciones",
            "medicina", "doctor", "enfermedad", "dolor", "sintoma", "síntoma", "hospital",
            "historia", "guerra", "rey", "reina", "imperio", "batalla", "antiguo",
            "geografia", "geografía", "pais", "país", "ciudad", "capital", "continente"
        };
        
        // Si contiene términos que definitivamente no son de IA, rechazar inmediatamente
        foreach (var keyword in nonAIKeywords)
        {
            if (message.Contains(keyword))
            {
                return false;
            }
        }
        
        // Palabras clave relacionadas con IA
        var aiKeywords = new[]
        {
            // IA básica
            "inteligencia artificial", "ia", "ai", "artificial intelligence",
            "machine learning", "aprendizaje automatico", "aprendizaje automático",
            "deep learning", "aprendizaje profundo",
            
            // Conceptos técnicos
            "red neuronal", "redes neuronales", "neural network", "neurona artificial",
            "algoritmo", "modelo", "entrenamiento", "datos", "dataset", "prediccion", "predicción",
            "clasificacion", "clasificación", "regresion", "regresión", "clustering",
            
            // Aplicaciones de IA
            "chatbot", "bot", "asistente virtual", "vision por computadora", "computer vision",
            "procesamiento de lenguaje natural", "nlp", "reconocimiento", "automatizacion", "automatización",
            
            // Tecnologías específicas
            "tensorflow", "pytorch", "keras", "scikit-learn", "python", "opencv",
            "big data", "data science", "ciencia de datos", "mineria de datos",
            
            // Conceptos avanzados
            "gan", "transformer", "bert", "gpt", "lstm", "cnn", "rnn",
            "reinforcement learning", "aprendizaje por refuerzo", "supervised learning",
            "unsupervised learning", "semi-supervised", "transfer learning",
            
            // Ética y sociedad
            "sesgo", "bias", "etica", "ética", "explicabilidad", "transparencia",
            "superinteligencia", "singularidad", "agi",
            
            // Industrias y aplicaciones
            "robótica", "robotica", "robot", "drone", "vehiculo autonomo", "vehículo autónomo",
            "medicina artificial", "diagnostico", "diagnóstico", "recomendador",
            
            // Prompt engineering y LLMs
            "prompt", "llm", "gpt", "chatgpt", "language model", "modelo de lenguaje"
        };
        
        // Verificar si contiene alguna palabra clave de IA
        foreach (var keyword in aiKeywords)
        {
            if (message.Contains(keyword))
            {
                return true;
            }
        }
        
        // También verificar si la pregunta está en la knowledge base
        var faqs = _kbService.GetAllFAQs();
        foreach (var faq in faqs)
        {
            // Si la pregunta es muy similar a alguna en la KB, probablemente sea de IA
            if (CalculateSimilarity(message, faq.question.ToLower()) > 0.6)
            {
                return true;
            }
        }
        
        // Si no encuentra coincidencias claras, por defecto rechazar (más restrictivo)
        return false;
    }
    
    private double CalculateSimilarity(string text1, string text2)
    {
        // Implementación simple de similitud basada en palabras comunes
        var words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var commonWords = words1.Intersect(words2).Count();
        var totalWords = words1.Union(words2).Count();
        
        return totalWords == 0 ? 0 : (double)commonWords / totalWords;
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
