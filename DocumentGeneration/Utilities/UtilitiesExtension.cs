using DocumentGeneration.Models;
using Razor.Templating.Core;

namespace DocumentGeneration.Utilities
{
    public static class UtilitiesExtension
    {
        public static async Task<string> GenerateHtmlContent(InvoiceData invoice, string viewName)
        {
            var html = await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", invoice);
            return html;
        }

        public static async Task<string> GenerateHtmlContent<T>(T invoice, string viewName)
        {
            return await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", invoice);
        }
    }
}
