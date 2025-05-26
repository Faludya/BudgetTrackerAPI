using ClosedXML.Excel;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Globalization;

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
    }
}
