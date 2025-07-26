using Markdig;

namespace MiChatbotBlazor.Services
{
    public class MarkdownService : IMarkdownService
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownService()
        {
            // Configurar el pipeline de Markdig con extensiones útiles
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions() // Incluye tablas, listas de tareas, etc.
                .UseEmojiAndSmiley() // Soporte para emojis
                .UseSoftlineBreakAsHardlineBreak() // Saltos de línea más naturales
                .Build();
        }

        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            return Markdown.ToHtml(markdown, _pipeline);
        }
    }
}
