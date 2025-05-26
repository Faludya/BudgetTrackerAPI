using Models.DTOs;

namespace Services.Interfaces
{
    public interface IImportParserService
    {
        Task<List<ParsedTransactionDto>> ParseAsync(Stream fileStream, string template);
    }

}
