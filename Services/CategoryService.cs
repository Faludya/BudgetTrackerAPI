using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class CategoryService : ICategoryService
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public CategoryService(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _repositoryWrapper.CategoryRepository.FindAll();
    }

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        return await _repositoryWrapper.CategoryRepository.GetCategoryById(id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category), "Category cannot be null.");
        }

        await _repositoryWrapper.CategoryRepository.Create(category);
        await _repositoryWrapper.Save();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category), "Category cannot be null.");
        }

        var existingCategory = await _repositoryWrapper.CategoryRepository.GetCategoryById(category.Id);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException($"Category with ID {category.Id} not found.");
        }

        existingCategory.Name = category.Name;
        existingCategory.IsPredefined = category.IsPredefined;
        existingCategory.ParentCategoryId = category.ParentCategoryId;
        
        await _repositoryWrapper.CategoryRepository.Update(existingCategory);
        await _repositoryWrapper.Save();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _repositoryWrapper.CategoryRepository.GetCategoryById(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        await _repositoryWrapper.CategoryRepository.Delete(category);
        await _repositoryWrapper.Save();
    }
}

}
