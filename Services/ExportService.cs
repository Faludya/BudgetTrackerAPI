using ClosedXML.Excel;
using Models;
using Models.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Services
{
    public static class ExportService
    {
        public static ExportFileResult ExportToExcel(IEnumerable<Transaction> transactions)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Transactions");

            // Headers
            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Category";
            worksheet.Cell(1, 3).Value = "Type";
            worksheet.Cell(1, 4).Value = "Amount";
            worksheet.Cell(1, 5).Value = "Currency";
            worksheet.Cell(1, 6).Value = "Description";

            var row = 2;
            foreach (var tx in transactions)
            {
                worksheet.Cell(row, 1).Value = tx.Date;
                worksheet.Cell(row, 2).Value = tx.Category.Name;
                worksheet.Cell(row, 3).Value = tx.Type;
                worksheet.Cell(row, 4).Value = tx.ConvertedAmount ?? tx.Amount;
                worksheet.Cell(row, 5).Value = tx.Currency?.Name ?? "-";
                worksheet.Cell(row, 6).Value = tx.Description;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return new ExportFileResult { Content = stream.ToArray() };
        }

        public static ExportFileResult ExportToPdf(IEnumerable<Transaction> transactions)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER TITLE
                    page.Header().Row(row =>
                    {
                        row.RelativeColumn()
                           .Text("Budget Tracker – Transaction Report")
                           .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                    });

                    // MAIN TABLE
                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);  // Date
                            columns.RelativeColumn();    // Category
                            columns.ConstantColumn(60);  // Type
                            columns.ConstantColumn(80);  // Amount
                            columns.ConstantColumn(50);  // Currency
                            columns.RelativeColumn();    // Description
                        });

                        // HEADER ROW
                        table.Header(header =>
                        {
                            static IContainer Style(IContainer container) => container
                                .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.Blue.Darken4))
                                .Background(Colors.Blue.Lighten4)
                                .PaddingVertical(5).PaddingHorizontal(8);

                            header.Cell().Element(Style).Text("Date");
                            header.Cell().Element(Style).Text("Category");
                            header.Cell().Element(Style).Text("Type");
                            header.Cell().Element(Style).Text("Amount");
                            header.Cell().Element(Style).Text("Curr.");
                            header.Cell().Element(Style).Text("Description");
                        });

                        // TABLE ROWS
                        bool isEven = true;
                        foreach (var tx in transactions)
                        {
                            var bg = isEven ? Colors.Grey.Lighten4 : Colors.White;
                            isEven = !isEven;

                            table.Cell().Background(bg).Padding(5).Text(tx.Date.ToString("yyyy-MM-dd"));

                            table.Cell().Background(bg).Padding(5).Row(row =>
                            {
                                row.RelativeColumn().Text(tx.Category.Name);
                            });

                            table.Cell().Background(bg).Padding(5)
                                .Text(tx.Type)
                                .FontColor(tx.Type?.ToLower() == "debit" ? Colors.Red.Lighten2 : Colors.Green.Lighten2);

                            table.Cell().Background(bg).Padding(5)
                                .Text((tx.ConvertedAmount ?? tx.Amount).ToString("F2"));

                            table.Cell().Background(bg).Padding(5).Text(tx.Currency?.Name ?? "-");

                            table.Cell().Background(bg).Padding(5).Text(tx.Description ?? "");
                        }

                        // TOTAL ROW
                        var total = transactions.Sum(t => t.ConvertedAmount ?? t.Amount);

                        table.Cell().ColumnSpan(3).AlignRight().PaddingTop(10).Text("Total:").SemiBold();
                        table.Cell().ColumnSpan(3).PaddingTop(10).Text($"{total:F2}").SemiBold();
                    });

                    // FOOTER
                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn()
                           .AlignLeft()
                           .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}")
                           .FontSize(10)
                           .FontColor(Colors.Grey.Darken2);

                        row.ConstantColumn(100)
                           .AlignRight()
                           .Text(text =>
                           {
                               text.Span("Page ");
                               text.CurrentPageNumber();
                               text.Span(" of ");
                               text.TotalPages();
                           });
                    });
                });
            }).GeneratePdf();

            return new ExportFileResult
            {
                Content = pdfBytes
            };
        }




    }
}
