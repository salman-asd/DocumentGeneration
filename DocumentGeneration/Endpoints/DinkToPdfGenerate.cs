using DinkToPdf;
using DocumentGeneration.Data;
using DocumentGeneration.Models;
using Razor.Templating.Core;

namespace DocumentGeneration.Endpoints;

public static class DinkToPdfGenerate
{
    public static IEndpointRouteBuilder MapDinkToPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("dinkToPdf");

        group.MapGet("get-invoice-pdf", async (int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Initialize DinkToPdf converter
            var converter = new SynchronizedConverter(new PdfTools());

            // Create HTML content from template
            string htmlContent = await GenerateInvoiceHtml(invoiceData, "invoice_dink");

            // Set DinkToPdf converter settings with improved configuration
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    //Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                    //DPI = 300, // Higher DPI for better quality
                    //DPI = 96, // Higher DPI for better quality
                    DocumentTitle = $"Invoice #{invoiceData.InvoiceNumber}",
                    Out = null, // Output directly to memory
                    UseCompression = true, // Compress output
                },
                Objects = {
                    new ObjectSettings
                    {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = {
                            DefaultEncoding = "utf-8",
                            EnableJavascript = true,
                            EnableIntelligentShrinking = true,
                            PrintMediaType = true, // Use print media type
                            //BackgroundColor = new BackgroundConfig { Color = "#FFFFFF" }, // Ensure white background
                        },
                        HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = false },
                        FooterSettings = { FontSize = 9, Line = false, Center = "Invoice generated on " + DateTime.Now.ToString("yyyy-MM-dd") },
                    }
                }
            };

            // Generate PDF
            byte[] pdfBytes = converter.Convert(doc);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("dinkToPdf-get-invoice-pdf")
        .WithOpenApi();

        group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
        {
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Create HTML content from template
            string htmlContent = await GenerateInvoiceHtml(invoiceData, "invoice_dink");
            return Results.Content(htmlContent, "text/html");
        })
        .WithName("dinkToPdf-get-invoice-preview")
        .WithOpenApi();

        return routes;
    }

    private static async Task<string> GenerateInvoiceHtml(InvoiceData invoice, string viewName)
    {
        var html = await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", invoice);
        return html;
    }
}
