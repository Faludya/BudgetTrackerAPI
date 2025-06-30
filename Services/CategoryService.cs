using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IWebHostEnvironment _env;

        public CategoryService(IRepositoryWrapper repositoryWrapper, IWebHostEnvironment webHostEnvironment)
        {
            _repositoryWrapper = repositoryWrapper;
            _env = webHostEnvironment;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(string userId)
        {
            return await _repositoryWrapper.CategoryRepository.GetAllCategoriesByUserId(userId);
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
            existingCategory.ColorHex = category.ColorHex;
            existingCategory.IconName = category.IconName;
            existingCategory.OrderIndex = category.OrderIndex;

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

        public async Task ReorderCategoriesAsync(List<CategoryReorderDto> reorderedCategories)
        {
            var categoryIds = reorderedCategories.Select(c => c.Id).ToList();
            var categoriesQuery = await _repositoryWrapper.CategoryRepository.FindByCondition(c => categoryIds.Contains(c.Id));
            var categories = await categoriesQuery.ToListAsync();

            foreach (var category in categories)
            {
                var reorderData = reorderedCategories.First(c => c.Id == category.Id);
                category.OrderIndex = reorderData.OrderIndex;
            }

            await _repositoryWrapper.Save();
        }

        public async Task<string?> PredictCategoryAsync(string description)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"predict_category.py \"{description}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                throw new Exception($"Prediction script error: {error}");

            var predictedLine = output.Split('\n').LastOrDefault(l => l.Contains("Predicted Category:"));
            var category = predictedLine?.Split(':').LastOrDefault()?.Trim();
            return category;
        }

        public async Task CreateDefaultCategories(string userId)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "..", "Services", "Files", "categories-defaults.json");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Default categories JSON not found.");

            var json = await File.ReadAllTextAsync(filePath);
            var templates = JsonSerializer.Deserialize<List<CategoryTemplate>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            var now = DateTime.UtcNow;
            var allCategories = new List<Category>();

            foreach (var parent in templates)
            {
                var parentCategory = new Category
                {
                    Name = parent.Name,
                    ColorHex = parent.ColorHex,
                    IconName = parent.IconName,
                    OrderIndex = parent.OrderIndex,
                    CategoryType = parent.CategoryType,
                    IsPredefined = true,
                    CreatedAt = now,
                    UserId = userId
                };

                allCategories.Add(parentCategory);

                if (parent.Subcategories != null)
                {
                    foreach (var sub in parent.Subcategories)
                    {
                        allCategories.Add(new Category
                        {
                            Name = sub.Name,
                            ColorHex = parent.ColorHex,
                            IconName = parent.IconName,
                            OrderIndex = 0,
                            CategoryType = parent.CategoryType,
                            IsPredefined = true,
                            CreatedAt = now,
                            UserId = userId,
                            ParentCategory = parentCategory
                        });
                    }
                }
            }

            foreach (var category in allCategories)
            {
                await _repositoryWrapper.CategoryRepository.Create(category);
            }
            await _repositoryWrapper.Save();
        }
    }

}
