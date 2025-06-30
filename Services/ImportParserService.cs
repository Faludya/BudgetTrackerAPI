using ClosedXML.Excel;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using System.IO.Compression;
using System.Xml.Linq;

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
                "brd-pdf" => await ParseBrdPdfAsync(fileStream),
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

            string currency = "EUR";

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
                HasHeaderRecord = false,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            });

            bool headerFound = false;
            string currency = "EUR"; // ✅ Default fallback if nothing found
            bool currencyExtracted = false;

            while (await csv.ReadAsync())
            {
                var firstCell = csv.GetField(0)?.Trim();

                // 🔹 Detect the header row (start of transactions)
                if (!headerFound && string.Equals(firstCell, "Data", StringComparison.OrdinalIgnoreCase))
                {
                    headerFound = true;
                    continue;
                }

                if (!headerFound)
                    continue;

                // 🔹 Try to extract currency from "Balanta" column
                if (!currencyExtracted)
                {
                    var balanceField = csv.GetField(7);
                    if (!string.IsNullOrWhiteSpace(balanceField))
                    {
                        var match = Regex.Match(balanceField, @"\b([A-Z]{3})\b");
                        if (match.Success)
                        {
                            currency = match.Value;
                            currencyExtracted = true;
                        }
                    }
                }

                var dateStr = csv.GetField(0)?.Trim();
                var description = csv.GetField(3)?.Trim();
                var debitStr = csv.GetField(4)?.Trim();
                var creditStr = csv.GetField(6)?.Trim();

                if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(description))
                    continue;

                // Try parsing multiple formats: "25 mai 2025" or "25-05-2025"
                if (!DateTime.TryParseExact(dateStr, "dd MMM yyyy", new CultureInfo("ro-RO"), DateTimeStyles.None, out var date) &&
                    !DateTime.TryParse(dateStr, new CultureInfo("ro-RO"), DateTimeStyles.None, out date))
                {
                    continue;
                }

                // Normalize amounts using invariant culture
                bool hasDebit = decimal.TryParse(debitStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                bool hasCredit = decimal.TryParse(creditStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);

                if (!hasDebit && !hasCredit)
                    continue;

                // ✅ Debit = negative (expense), Credit = positive (income)
                var amount = hasDebit ? -debit : credit;

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

        private async Task<List<ParsedTransactionDto>> ParseIngExcelAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();
            var cleanStream = RemoveDrawingsFromXlsxStream(stream);
            using var workbook = new XLWorkbook(cleanStream);
            var sheet = workbook.Worksheet(1);

            string currency = "EUR"; // default
            bool currencyExtracted = false;

            foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() >= 6))
            {
                var dateCell = row.Cell(1);
                var descCell = row.Cell(2);
                var debitCell = row.Cell(3);
                var creditCell = row.Cell(4);
                var balanceCell = row.Cell(5);

                var desc = descCell.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(desc)) continue;

                // Try to extract currency once from balance
                if (!currencyExtracted)
                {
                    var balance = balanceCell.GetString();
                    var match = Regex.Match(balance, @"\b([A-Z]{3})\b");
                    if (match.Success)
                    {
                        currency = match.Value;
                        currencyExtracted = true;
                    }
                }

                if (!dateCell.TryGetValue(out DateTime date)) continue;
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                var debitStr = debitCell.GetString()?.Trim();
                var creditStr = creditCell.GetString()?.Trim();

                bool hasDebit = decimal.TryParse(debitStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var debit);
                bool hasCredit = decimal.TryParse(creditStr?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var credit);

                if (!hasDebit && !hasCredit) continue;

                var amount = hasDebit ? -debit : credit;

                results.Add(new ParsedTransactionDto
                {
                    Date = date,
                    Description = desc,
                    Amount = amount,
                    Currency = currency,
                    Category = null
                });
            }

            return results;
        }

        private async Task<List<ParsedTransactionDto>> ParseBrdPdfAsync(Stream stream)
        {
            var results = new List<ParsedTransactionDto>();

            using var document = PdfDocument.Open(stream);
            var fullText = string.Join("\n", document.GetPages().Select(p => p.Text));

            // Match: amount + optional description + date (dd/MM/yyyy) + optional second date
            var transactionRegex = new Regex(
                @"(?<amount>\d{1,3}(?:\.\d{3})*,\d{2})" + // amount like 1.500,00
                @"(?<desc>.+?)" +                        // description (non-greedy)
                @"(?<date1>\d{2}/\d{2}/\d{4})" +         // first date (value date)
                @"(?<date2>\d{2}/\d{2}/\d{4})",          // second date (transaction date)
                RegexOptions.Singleline);

            var matches = transactionRegex.Matches(fullText);

            foreach (Match match in matches)
            {
                var amountStr = match.Groups["amount"].Value.Trim();
                var description = match.Groups["desc"].Value.Trim();
                var dateStr = match.Groups["date1"].Value.Trim();

                if (!decimal.TryParse(amountStr.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    continue;

                if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    continue;

                // Use last few words of description
                var descLines = Regex.Split(description, @"(?<!\d)\s+(?=\S)"); // split on blocks of spaces
                int cardIndex = Array.FindIndex(descLines, line => line.Contains("Card nr", StringComparison.OrdinalIgnoreCase));
                string shortDescription = (cardIndex >= 0 && cardIndex + 1 < descLines.Length)
                    ? descLines[cardIndex + 1].Trim()
                    : descLines.LastOrDefault()?.Trim() ?? "Unknown";



                // Heuristic: is it credit or debit?
                var isCredit = description.Contains("Incasare", StringComparison.OrdinalIgnoreCase) ||
                               description.Contains("Transfer", StringComparison.OrdinalIgnoreCase) ||
                               description.Contains("Plata instant", StringComparison.OrdinalIgnoreCase);

                if (!isCredit)
                    amount *= -1;

                results.Add(new ParsedTransactionDto
                {
                    Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                    Description = shortDescription,
                    Amount = amount,
                    Currency = "EUR",
                    Category = null
                });
            }

            return results;
        }



        private Stream RemoveDrawingsFromXlsxStream(Stream originalStream)
        {
            var cleanedStream = new MemoryStream();
            using var archive = new ZipArchive(originalStream, ZipArchiveMode.Read, leaveOpen: true);
            using var newArchiveStream = new MemoryStream();
            using (var newArchive = new ZipArchive(newArchiveStream, ZipArchiveMode.Create, true))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip drawings/media/charts completely
                    if (entry.FullName.StartsWith("xl/media/", StringComparison.OrdinalIgnoreCase) ||
                        entry.FullName.StartsWith("xl/drawings/", StringComparison.OrdinalIgnoreCase) ||
                        entry.FullName.StartsWith("xl/charts/", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Clean .rels entries properly
                    if (entry.FullName.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
                    {
                        using var entryStream = entry.Open();
                        var xml = XDocument.Load(entryStream);

                        // Remove Relationship nodes that point to drawings/media/charts
                        var relationships = xml.Root?.Elements()
                            .Where(e =>
                                e.Attribute("Target")?.Value.Contains("drawing", StringComparison.OrdinalIgnoreCase) == true ||
                                e.Attribute("Target")?.Value.Contains("media", StringComparison.OrdinalIgnoreCase) == true ||
                                e.Attribute("Target")?.Value.Contains("chart", StringComparison.OrdinalIgnoreCase) == true
                            )
                            .ToList();

                        if (relationships != null)
                        {
                            foreach (var r in relationships)
                                r.Remove();
                        }

                        var newEntry = newArchive.CreateEntry(entry.FullName);
                        using var writer = new StreamWriter(newEntry.Open());
                        xml.Save(writer);
                        continue;
                    }

                    // Copy everything else as-is
                    var newFile = newArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                    using var originalFileStream = entry.Open();
                    using var newFileStream = newFile.Open();
                    originalFileStream.CopyTo(newFileStream);
                }
            }

            newArchiveStream.Seek(0, SeekOrigin.Begin);
            cleanedStream.Seek(0, SeekOrigin.Begin);
            newArchiveStream.CopyTo(cleanedStream);
            cleanedStream.Seek(0, SeekOrigin.Begin);
            return cleanedStream;
        }

    }
}
