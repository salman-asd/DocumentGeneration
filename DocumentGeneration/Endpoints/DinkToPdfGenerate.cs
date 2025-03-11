using DinkToPdf;
using DinkToPdf.Contracts;
using DocumentGeneration.Data;
using DocumentGeneration.Models;
using DocumentGeneration.Utilities;
using Microsoft.AspNetCore.Mvc;
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
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent(invoiceData, "invoice_dink");

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

        //group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
        //{
        //    var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

        //    // Create HTML content from template
        //    string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_puppeteer");
        //    return Results.Content(htmlContent, "text/html");
        //})
        //.WithName("dinkToPdf-get-invoice-preview")
        //.WithOpenApi();

        group.MapGet("get-invoice-pdf2", async ([FromServices] IConverter converter, int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Create HTML content from template
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_puppeteer");

            // Generate PDF using DinkToPdf
            byte[] pdfBytes = GeneratePdfFromHtml(htmlContent, converter);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("dinkToPdf-get-invoice-pdf2")
        .WithOpenApi();


        group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
        {
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_dinktopdf");

            return Results.Content(htmlContent, "text/html");
        })
        .WithName("dinkToPdf-get-invoice-preview")
        .WithOpenApi();

        return routes;
    }


    private static byte[] GeneratePdfFromHtml(string htmlContent, IConverter converter)
    {
        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 60, Bottom = 40 },
            DocumentTitle = "Invoice PDF"
        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = htmlContent,
            WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true },
            HeaderSettings = { FontSize = 12, Center = "Invoice Details", Spacing = 5 },
            FooterSettings = { FontSize = 10, Left = $"Printed on {DateTime.Now:yyyy-MM-dd HH:mm:ss}", Right = "[page] of [toPage]", Spacing = 5 }
        };

        var pdf = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        return converter.Convert(pdf);
    }
}
