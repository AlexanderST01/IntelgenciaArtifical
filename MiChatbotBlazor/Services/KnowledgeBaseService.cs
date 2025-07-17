using System.Text.Json;
using System.Text.RegularExpressions;

namespace MiChatbotBlazor.Services
{
    public class KnowledgeBaseService
    {
        private readonly List<FAQItem> _faqItems;
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
            // Búsqueda exacta o por similitud básica (puedes mejorar con NLP)
            var normalized = Normalize(userQuestion);
            foreach (var item in _faqItems)
            {
                if (Normalize(item.Question) == normalized || normalized.Contains(Normalize(item.Question)))
                    return item.Answer;
            }
            return null;
        }

        public bool IsAboutAI(string userQuestion)
        {
            // Palabras clave básicas para IA
            var keywords = new[] { "inteligencia artificial", "ia", "machine learning", "aprendizaje automático", "deep learning", "red neuronal", "modelo de lenguaje", "chatbot" };
            var normalized = Normalize(userQuestion);
            return keywords.Any(k => normalized.Contains(k));
        }

        private string Normalize(string text)
        {
            return Regex.Replace(text.ToLowerInvariant(), "[áéíóúüñ]", m => m.Value.Normalize(System.Text.NormalizationForm.FormD)[0].ToString())
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u").Replace("ñ", "n").Trim();
        }

        public class FAQItem
        {
            public string Question { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
        }
    }
}
