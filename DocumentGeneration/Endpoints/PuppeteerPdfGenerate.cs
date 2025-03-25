using DocumentGeneration.Data;
using DocumentGeneration.Models;
using DocumentGeneration.Utilities;
using PuppeteerSharp.Media;
using PuppeteerSharp;

namespace DocumentGeneration.Endpoints
{
    public static class PuppeteerPdfGenerate
    {
        public static IEndpointRouteBuilder MapPuppeteerPdfGenerate(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("puppeteerPdf");

            group.MapGet("get-invoice-pdf", async (int? lineItemCount = 10) =>
            {
                // Generate invoice data
                var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

                // Create HTML content from template
                string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice");

                // Generate PDF
                // Convert HTML to PDF using PuppeteerSharp
                byte[] pdfBytes = await GeneratePdfFromHtml(htmlContent);

                return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
            })
            .WithName("puppeteerPdf-get-invoice-pdf")
            .WithOpenApi();

            group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
            {
                var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

                // Create HTML content from template
                string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice");

                return Results.Content(htmlContent, "text/html");
            })
            .WithName("puppeteer-get-invoice-preview")
            .WithOpenApi();

            return routes;
        }

        private static async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
        {
            // Ensure Puppeteer is downloaded
            await new BrowserFetcher().DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();

            // Load HTML content
            await page.SetContentAsync(htmlContent);

            // Get current date and format it
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Generate PDF with header and footer
            return await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                DisplayHeaderFooter = true,
                HeaderTemplate = @"
                    <div style='width:100%; font-size:12px; padding:10px 20px; font-weight:bold;'>
                        Invoice Details
                    </div>",
                FooterTemplate = $@"
                    <div style='width:100%; font-size:10px; padding:10px 20px; display:flex; justify-content:space-between;'>
                        <span style='text-align:left;'>Printed on {currentDateTime}</span>
                        <span style='text-align:right;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
                    </div>",
                MarginOptions = new MarginOptions
                {
                    Top = "60px",  // Ensure enough space for the header
                    Bottom = "40px" // Ensure enough space for the footer
                }
            });
        }
    }
}
