using Models.DTOs;
using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class CategoryKeywordMappingService : ICategoryKeywordMappingService
    {
        private readonly IRepositoryWrapper _repository;

        public CategoryKeywordMappingService(IRepositoryWrapper repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryKeywordMapping>> GetAllForUserAsync(string userId)
        {
            return await _repository.CategoryKeywordMappingRepository.GetAllForUserAsync(userId);
        }

        public async Task<CategoryKeywordMapping?> GetByIdAsync(int id)
        {
            return await _repository.CategoryKeywordMappingRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(string userId, CategoryKeywordMappingDto dto)
        {
            var existing = await _repository.CategoryKeywordMappingRepository.GetByKeywordAsync(userId, dto.Keyword);
            if (existing != null)
                throw new InvalidOperationException("Mapping already exists.");

            var mapping = new CategoryKeywordMapping
            {
                UserId = userId,
                Keyword = dto.Keyword.ToLower(),
                CategoryId = dto.CategoryId
            };

            await _repository.CategoryKeywordMappingRepository.Create(mapping);
            await _repository.Save();
        }

        public async Task<bool> UpdateAsync(string userId, UpdateCategoryKeywordMappingDto dto)
        {
            var existing = await _repository.CategoryKeywordMappingRepository.GetByIdAsync(dto.Id);
            if (existing == null || existing.UserId != userId)
                return false;

            existing.Keyword = dto.Keyword.ToLower();
            existing.CategoryId = dto.CategoryId;

            _repository.CategoryKeywordMappingRepository.Update(existing);
            await _repository.Save();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, int id)
        {
            var existing = await _repository.CategoryKeywordMappingRepository.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
                return false;

            _repository.CategoryKeywordMappingRepository.Delete(existing);
            await _repository.Save();
            return true;
        }

        // Utility for ImportService to find a category suggestion
        public async Task<int?> FindCategoryIdByKeywordAsync(string userId, string description)
        {
            var words = description
                .ToLower()
                .Split(new[] { ' ', '|', '-', ',', '.', ':' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3);

            foreach (var word in words)
            {
                var mapping = await _repository.CategoryKeywordMappingRepository.GetByKeywordAsync(userId, word);
                if (mapping != null)
                    return mapping.CategoryId;
            }

            return null;
        }
    }

}
