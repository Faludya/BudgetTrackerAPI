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

            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Transaction Report")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  // Date
                                columns.RelativeColumn();    // Category
                                columns.ConstantColumn(60);  // Type
                                columns.ConstantColumn(80);  // Amount
                                columns.ConstantColumn(50);  // Currency
                                columns.RelativeColumn();    // Description
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date");
                                header.Cell().Element(CellStyle).Text("Category");
                                header.Cell().Element(CellStyle).Text("Type");
                                header.Cell().Element(CellStyle).Text("Amount");
                                header.Cell().Element(CellStyle).Text("Curr.");
                                header.Cell().Element(CellStyle).Text("Description");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).Padding(5).Background(Colors.Grey.Lighten2);
                                }
                            });

                            // Table Rows
                            foreach (var tx in transactions)
                            {
                                table.Cell().Element(Cell => Cell.Padding(5)).Text(tx.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(Cell => Cell.Padding(5)).Text(tx.Category.Name);
                                table.Cell().Element(Cell => Cell.Padding(5)).Text(tx.Type);
                                table.Cell().Element(Cell => Cell.Padding(5)).Text((tx.ConvertedAmount ?? tx.Amount).ToString("F2"));
                                table.Cell().Element(Cell => Cell.Padding(5)).Text(tx.Currency?.Name ?? "-");
                                table.Cell().Element(Cell => Cell.Padding(5)).Text(tx.Description ?? "");
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            }).GeneratePdf();

            return new ExportFileResult
            {
                Content = pdfBytes
            };
        }

    }
}
