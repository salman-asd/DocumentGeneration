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

        group.MapGet("get-invoice-pdf2", (int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // -----------------------------
                    // Header (single definition)
                    // -----------------------------
                    page.Header().Column(headerColumn =>
                    {
                        // Top centered Invoice title
                        //headerColumn.Item().AlignCenter().Text("Invoice")
                        //    .FontSize(20).Bold()
                        //    .PaddingBottom(10); // add some spacing

                        // Header row: left (company) and right (invoice info)
                        headerColumn.Item().Row(row =>
                        {
                            // Left side: company info
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("EasyPOS")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                column.Item().Text("Gorczany - Mitchell");
                                column.Item().Text("566 Jovan Shoals, East Edythe");
                                column.Item().Text("PA, 42103-3716, Eritrea");
                            });

                            // Right side: invoice details
                            row.RelativeItem().AlignRight().Column(column =>
                            {
                                column.Item().Text("INVOICE")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                column.Item().Text($"Invoice Number: {invoiceData.InvoiceNumber}");
                                column.Item().Text($"Date of Issue: {invoiceData.InvoiceDate:yyyy-MM-dd}");
                                column.Item().Text($"Due Date: {invoiceData.DueDate:yyyy-MM-dd}");
                            });
                        });
                    });

                    // -----------------------------
                    // Main Content
                    // -----------------------------
                    page.Content().Column(column =>
                    {
                        // INVOICE TO:
                        column.Item().PaddingVertical(15).Column(innerCol =>
                        {
                            innerCol.Item().Text("INVOICE TO:")
                                .Bold().FontColor(Colors.Blue.Medium);
                            innerCol.Item().Text(invoiceData.ClientName);
                            innerCol.Item().Text(invoiceData.ClientAddress);
                            innerCol.Item().Text($"{invoiceData.ClientCity}, {invoiceData.ClientState}, {invoiceData.ClientPostal}");
                            innerCol.Item().Text(invoiceData.ClientCountry);
                        });

                        // Line Items Table
                        column.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // Description
                                columns.RelativeColumn(1); // Qty
                                columns.RelativeColumn(2); // Unit Price
                                columns.RelativeColumn(2); // Total
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Description").Bold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Qty").Bold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Unit Price").Bold().AlignRight();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Total").Bold().AlignRight();
                            });

                            // Table Content
                            foreach (var item in invoiceData.LineItems)
                            {
                                decimal price = decimal.Parse(item.Price, CultureInfo.InvariantCulture);
                                decimal total = price * item.Quantity;

                                table.Cell().Padding(5).Text(item.Name);
                                table.Cell().Padding(5).Text(item.Quantity.ToString()).AlignCenter();
                                table.Cell().Padding(5).Text($"${price:F2}").AlignRight();
                                table.Cell().Padding(5).Text($"${total:F2}").AlignRight();
                            }
                        });

                        // Summary Table (Right-Aligned), near the bottom
                        column.Item().PaddingTop(20).AlignRight().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Cell().Text("Subtotal").Bold();
                            table.Cell().Text($"${invoiceData.Subtotal:F2}").AlignRight();

                            table.Cell().Text("Discount").Bold();
                            table.Cell().Text($"${invoiceData.Discount:F2}").AlignRight();

                            table.Cell().Text("Subtotal Less Discount").Bold();
                            table.Cell().Text($"${invoiceData.SubtotalLessDiscount:F2}").AlignRight();

                            table.Cell().Text("Tax (0.1%)").Bold();
                            table.Cell().Text($"${invoiceData.TaxTotal:F2}").AlignRight();

                            table.Cell().Text("Balance Due").Bold().FontColor(Colors.Blue.Medium);
                            table.Cell().Text($"${invoiceData.BalanceDue:F2}")
                                .Bold().AlignRight().FontColor(Colors.Blue.Medium);
                        });

                        // Thank you / Payment Info
                        column.Item().PaddingTop(15).Column(msgCol =>
                        {
                            msgCol.Item().Text("Thank you for your business!")
                                .SemiBold().FontColor(Colors.Blue.Medium);
                            msgCol.Item().Text("Payment is due within 30 days. Late payments may be subject to additional fees.")
                                .FontSize(9).Italic();
                        });
                    });

                    // -----------------------------
                    // Footer
                    // -----------------------------
                    page.Footer()
                        .AlignRight()
                        .Text($"Printed on {DateTime.Now:yyyy-MM-dd HH:mm:ss} Page 1 of 1")
                        .FontColor(Colors.Grey.Medium);
                });
            });

            var pdfStream = new MemoryStream();
            document.GeneratePdf(pdfStream);
            pdfStream.Position = 0;

            return Results.File(pdfStream, "application/pdf", "invoice.pdf");
        })
        .WithName("questPdf-get-invoice-pdf2")
        .WithOpenApi();

        return routes;
    }
}
