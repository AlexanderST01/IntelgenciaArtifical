using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiChatbotBlazor.Services
{
    public class KnowledgeBaseService
    {
        private readonly List<FAQItem> _faqItems;
        private readonly List<string> _greetings = new() { "hola", "buenos dias", "buenas tardes", "buenas noches", "saludos", "hello", "hi" };
        private const string GreetingResponse = "¡Hola! ¿En qué puedo ayudarte sobre inteligencia artificial?";

        public KnowledgeBaseService(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _faqItems = JsonSerializer.Deserialize<List<FAQItem>>(json) ?? new();
            }
            else
            {
                _faqItems = new List<FAQItem>();
            }
        }

        public string? GetAnswer(string userQuestion)
        {
            // Responder a saludos
            if (IsGreeting(userQuestion))
                return GreetingResponse;

            // Búsqueda exacta o por similitud básica (puedes mejorar con NLP)
            var normalized = Normalize(userQuestion);
            foreach (var item in _faqItems)
            {
                if (Normalize(item.question) == normalized || normalized.Contains(Normalize(item.question)))
                    return item.answer;
            }
            return null;
        }

        public List<FAQItem> GetAllFAQs() => _faqItems;

        public bool IsAboutAI(string userQuestion)
        {
            // Palabras clave básicas para IA
            var keywords = new[] { "inteligencia artificial", "ia", "machine learning", "aprendizaje automático", "deep learning", "red neuronal", "modelo de lenguaje", "chatbot" };
            var normalized = Normalize(userQuestion);
            return keywords.Any(k => normalized.Contains(k));
        }

        private bool IsGreeting(string userQuestion)
        {
            var normalized = Normalize(userQuestion);
            return _greetings.Any(g => normalized.StartsWith(g) || normalized == g);
        }

        private string Normalize(string text)
        {
            return Regex.Replace(text.ToLowerInvariant(), "[áéíóúüñ]", m => m.Value.Normalize(System.Text.NormalizationForm.FormD)[0].ToString())
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u").Replace("ñ", "n").Trim();
        }

        public class FAQItem
        {
            public string question { get; set; } = string.Empty;
            public string answer { get; set; } = string.Empty;
        }
    }
}
