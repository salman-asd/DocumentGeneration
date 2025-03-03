using DocumentGeneration.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace DocumentGeneration.Endpoints;

public static class QuestPdfGenerate
{
    public static IEndpointRouteBuilder MapQuestPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("questPdf");

        group.MapGet("get-invoice-pdf", (int? lineItemCount = 10) =>
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Header with dark blue background
                    page.Header().Height(20).Background("#2B4A58");

                    // Content section
                    page
                    .Content()
                    .PaddingHorizontal(30)
                    .Column(column =>
                    {
                        // Title and Logo section
                        column.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Text("INVOICE")
                                .FontSize(30)
                                .Bold()
                                .FontColor(Colors.Black);

                            //row.ConstantItem(100).AlignCenter().AlignMiddle().Circle()
                            //    .Width(80)
                            //    .Height(80)
                            //    .Background(Colors.Grey.Medium)
                            //    .Text("LOGO")
                            //    .FontColor(Colors.White)
                            //    .FontSize(16)
                            //    .Bold();
                        });

                        column.Item().PaddingTop(40);

                        // Company and client information section
                        column.Item().Row(row =>
                        {
                            // Your company information
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Company Name").Bold().FontColor(Color.FromHex("#2B4A58"));
                                c.Item().Text(invoiceData.CompanyAddress);
                                c.Item().Text($"{invoiceData.CompanyCity}, {invoiceData.CompanyState}");
                                c.Item().Text(invoiceData.CompanyCountry);
                                c.Item().Text(invoiceData.CompanyPostal);
                            });

                            // Invoice to section
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("INVOICE TO:").ExtraBold().FontColor(Color.FromHex("#2B4A58"));
                                c.Item().Text(invoiceData.ClientName);
                                c.Item().Text(invoiceData.ClientAddress);
                                c.Item().Text($"{invoiceData.ClientCity}, {invoiceData.ClientState}");
                                c.Item().Text(invoiceData.ClientCountry);
                                c.Item().Text(invoiceData.ClientPostal);
                            });

                            // Invoice details
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Invoice Number").ExtraBold().FontColor(Color.FromHex("#2B4A58"));
                                c.Item().Text($"#{invoiceData.InvoiceNumber}");
                                c.Item().Text("Date of Invoice").ExtraBold().FontColor(Color.FromHex("#2B4A58"));
                                c.Item().Text(invoiceData.InvoiceDate);
                                c.Item().Text("Due Date").ExtraBold().FontColor(Color.FromHex("#2B4A58"));
                                c.Item().Text(invoiceData.DueDate);
                            });
                        });

                        column.Item().PaddingTop(40);

                        // Line items table
                        column.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Background("#2B4A58").Padding(5)
                                    .Text("DESCRIPTION").FontColor(Colors.White).Bold();
                                header.Cell().Background("#2B4A58").Padding(5)
                                    .Text("QTY").FontColor(Colors.White).Bold();
                                header.Cell().Background("#2B4A58").Padding(5)
                                    .Text("UNIT PRICE").FontColor(Colors.White).Bold();
                                header.Cell().Background("#2B4A58").Padding(5)
                                    .Text("TOTAL").FontColor(Colors.White).Bold();
                            });

                            // Table content
                            foreach (var item in invoiceData.LineItems)
                            {
                                decimal price = decimal.Parse(item.Price, CultureInfo.InvariantCulture);
                                decimal total = price * item.Quantity;

                                // Alternate row backgrounds
                                var background = item.Index % 2 == 0 ? Colors.Grey.Lighten3 : Colors.Grey.Lighten4;

                                table.Cell().Background(background).Padding(5).Text(item.Name);
                                table.Cell().Background(background).Padding(5).Text(item.Quantity.ToString());
                                table.Cell().Background(background).Padding(5).Text($"${price:F2}");
                                table.Cell().Background(background).Padding(5).Text($"${total:F2}");
                            }
                        });

                        column.Item().PaddingTop(20);

                        // Summary section
                        column.Item().AlignRight().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Cell().Text("SUBTOTAL").Bold();
                            table.Cell().Text($"${invoiceData.Subtotal:F2}");

                            table.Cell().Text("DISCOUNT").Bold();
                            table.Cell().Text($"${invoiceData.Discount:F2}");

                            table.Cell().Text("SUBTOTAL LESS DISCOUNT").Bold();
                            table.Cell().Text($"${invoiceData.SubtotalLessDiscount:F2}");

                            table.Cell().Text("TAX RATE").Bold();
                            table.Cell().Text($"{invoiceData.TaxRate * 100:F0}%");

                            table.Cell().Text("TAX TOTAL").Bold();
                            table.Cell().Text($"${invoiceData.TaxTotal:F2}");

                            table.Cell().Text("BALANCE DUE").Bold();
                            table.Cell().Text($"${invoiceData.BalanceDue:F2}").Bold();
                        });

                        column.Item().PaddingTop(40);

                        // Notes and Terms
                        column.Item().Column(c =>
                        {
                            c.Item().Text("NOTES:").Bold();
                            c.Item().PaddingBottom(20).Text(invoiceData.Notes);

                            c.Item().Text("TERMS AND CONDITIONS:").Bold();
                            c.Item().Text(invoiceData.Terms);
                        });
                    });

                    // Footer with dark blue background and InvoiceBerry logo
                    page.Footer().Height(20).Row(row =>
                    {
                        row.RelativeItem().Background("#2B4A58");

                        // Add InvoiceBerry label
                        //row.ConstantItem(120).AlignBottom().AlignRight().PaddingBottom(10).PaddingRight(10)
                        //    .Row(r =>
                        //    {
                        //        //r.ConstantItem(20).Circle().Width(16).Height(16).Background("#4285F4");
                        //        r.ConstantItem(100).Text("InvoiceBerry").FontSize(10);
                        //    });
                    });
                });
            });

            var pdfStream = new MemoryStream();
            document.GeneratePdf(pdfStream);
            pdfStream.Position = 0;

            return Results.File(pdfStream, "application/pdf", "invoice.pdf");
        })
        .WithName("questPdf-get-invoice-pdf")
        .WithOpenApi();

        return routes;
    }
}
