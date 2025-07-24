using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiChatbotBlazor.Services
{
    public class KnowledgeBaseService
    {
        private readonly List<FAQItem> _faqItems;
        private readonly List<string> _greetings = new() { "hola", "buenos dias", "buenas tardes", "buenas noches", "saludos", "hello", "hi" };
        private const string GreetingResponse = "�Hola! �En qu� puedo ayudarte sobre inteligencia artificial?";

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

            // B�squeda exacta o por similitud b�sica (puedes mejorar con NLP)
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
            // Palabras clave b�sicas para IA
            var keywords = new[] { "inteligencia artificial", "ia", "machine learning", "aprendizaje autom�tico", "deep learning", "red neuronal", "modelo de lenguaje", "chatbot" };
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
            return Regex.Replace(text.ToLowerInvariant(), "[�������]", m => m.Value.Normalize(System.Text.NormalizationForm.FormD)[0].ToString())
                .Replace("�", "a").Replace("�", "e").Replace("�", "i").Replace("�", "o").Replace("�", "u").Replace("�", "u").Replace("�", "n").Trim();
        }

        public class FAQItem
        {
            public string question { get; set; } = string.Empty;
            public string answer { get; set; } = string.Empty;
        }
    }
}
