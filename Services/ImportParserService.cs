using ClosedXML.Excel;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Globalization;
using UglyToad.PdfPig;

namespace Services
{
    public class ImportParserService : IImportParserService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public ImportParserService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<ParsedTransactionDto>> ParseAsync(Stream fileStream, string template)
        {
            return template.ToLower() switch
            {
                "revolut" => await ParseRevolutAsync(fileStream),
                "raiffeisen" => await ParseRaiffeisenAsync(fileStream),
                "ing-csv" => await ParseIngCsvAsync(fileStream),
                "ing-excel" => await ParseIngExcelAsync(fileStream),
                "bcr-pdf" => await ParseBcrPdfAsync(fileStream),
                _ => throw new Exception("Unsupported template type.")
            };
        }


        private async Task<List<ParsedTransactionDto>> ParseRevolutAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);

            var results = new List<ParsedTransactionDto>();
            await csv.ReadAsync(); // Skip header
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var date = csv.GetField("Completed Date");
                var desc = csv.GetField("Description");
                var amt = csv.GetField("Amount");
                var curr = csv.GetField("Currency");

                if (DateTime.TryParse(date, out var parsedDate) &&
                    decimal.TryParse(amt, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    results.Add(new ParsedTransactionDto
                    {
                        Date = parsedDate,
                        Description = desc,
                        Amount = amount,
                        Currency = curr,
                        Category = null // this will be suggested later
                    });
                }
            }

            return results;
        }

        private async Task<List<ParsedTransactionDto>> ParseRaiffeisenAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            string currency = "RON";

            var valutaCell = sheet.CellsUsed()
                .FirstOrDefault(c => c.Value.ToString().Trim().Equals("Valuta:", StringComparison.OrdinalIgnoreCase))
                ?.CellRight();

            if (valutaCell != null && !string.IsNullOrWhiteSpace(valutaCell.GetString()))
            {
                currency = valutaCell.GetString().Trim();
            }

            // Loop starting from row 19
            foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() >= 19).OrderBy(r => r.RowNumber()))
            {
                // B = col 2 (transaction date)
                var dateCell = row.Cell(2).GetString()?.Trim();
                if (string.IsNullOrEmpty(dateCell)) continue;

                if (!DateTime.TryParseExact(dateCell, new[] { "dd/MM/yyyy", "dd.MM.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    continue;

                // C = col 3 (debit), D = col 4 (credit)
                var debitStr = row.Cell(3).GetString()?.Trim();
                var creditStr = row.Cell(4).GetString()?.Trim();

                var hasDebit = decimal.TryParse(debitStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                var hasCredit = decimal.TryParse(creditStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);

                if (!hasDebit && !hasCredit)
                    continue;

                var amount = hasDebit ? -debit : credit;

                // L = col 12 (description)
                var description = row.Cell(12).GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(description))
                    continue;

                results.Add(new ParsedTransactionDto
                {
                    Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                    Description = description,
                    Amount = amount,
                    Currency = currency,
                    Category = null
                });
            }

            return results;
        }

        private async Task<List<ParsedTransactionDto>> ParseIngCsvAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();
            using var reader = new StreamReader(stream);
            using var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(new CultureInfo("ro-RO"))
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null
            });

            // Skip the first two metadata rows manually
            for (int i = 0; i < 2; i++) await csv.ReadAsync();

            csv.Read(); // third line is header
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var dateRaw = csv.GetField("Data")?.Trim();
                var description = csv.GetField("Detalii tranzactie")?.Trim();
                var debitStr = csv.GetField("Debit")?.Trim();
                var creditStr = csv.GetField("Credit")?.Trim();

                if (string.IsNullOrWhiteSpace(dateRaw) || string.IsNullOrWhiteSpace(description)) continue;

                if (!DateTime.TryParseExact(dateRaw, "dd MMM yyyy", new CultureInfo("ro-RO"), DateTimeStyles.None, out var date))
                    continue;

                bool hasDebit = decimal.TryParse(debitStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                bool hasCredit = decimal.TryParse(creditStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);

                if (!hasDebit && !hasCredit) continue;

                var amount = hasDebit ? -debit : credit;

                results.Add(new ParsedTransactionDto
                {
                    Date = date,
                    Description = description,
                    Amount = amount,
                    Currency = "RON",
                    Category = null
                });
            }

            return results;
        }

        private async Task<List<ParsedTransactionDto>> ParseIngExcelAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            string currency = "RON"; // Assumed from the file; adjust if needed

            // Start from row 6 (after metadata and header)
            foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() >= 6))
            {
                var dateCell = row.Cell(1).GetDateTime();
                var desc = row.Cell(2).GetString()?.Trim();
                var debitStr = row.Cell(3).GetString()?.Trim();
                var creditStr = row.Cell(4).GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(desc)) continue;

                bool hasDebit = decimal.TryParse(debitStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                bool hasCredit = decimal.TryParse(creditStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);

                if (!hasDebit && !hasCredit) continue;

                var amount = hasDebit ? -debit : credit;

                results.Add(new ParsedTransactionDto
                {
                    Date = DateTime.SpecifyKind(dateCell, DateTimeKind.Utc),
                    Description = desc,
                    Amount = amount,
                    Currency = currency,
                    Category = null
                });
            }

            return results;
        }


        private async Task<List<ParsedTransactionDto>> ParseBcrPdfAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();

            using var document = PdfDocument.Open(stream);

            foreach (var page in document.GetPages())
            {
                var lines = page.Text
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                for (int i = 0; i < lines.Count - 3; i++)
                {
                    var amountLine = lines[i];

                    // Skip totals and headers
                    if (amountLine.Contains("Total") || amountLine.Contains("Sold") || amountLine.Length > 20)
                        continue;

                    // Step 1: Try parse amount (e.g. "153,15" or "1.500,00")
                    if (!decimal.TryParse(amountLine.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                        continue;

                    var description = lines[i + 1];

                    // Step 2: Look ahead for a valid date (e.g. 21/05/2025)
                    var dateLine = lines[i + 2];
                    if (!DateTime.TryParseExact(dateLine, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        continue;

                    // Step 3: Determine if it's a credit or debit (based on context: optional)
                    bool isCredit = false;

                    // Look forward for indicators like "Transfer", "Incasare", etc.
                    for (int j = i + 2; j < Math.Min(i + 6, lines.Count); j++)
                    {
                        if (lines[j].Contains("Incasare", StringComparison.OrdinalIgnoreCase) ||
                            lines[j].Contains("Transfer credit", StringComparison.OrdinalIgnoreCase))
                        {
                            isCredit = true;
                            break;
                        }
                    }

                    if (!isCredit) amount *= -1;

                    results.Add(new ParsedTransactionDto
                    {
                        Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                        Description = description,
                        Amount = amount,
                        Currency = "RON",
                        Category = null
                    });

                    i += 2; // Skip processed block
                }
            }

            return results;
        }
    }
}
