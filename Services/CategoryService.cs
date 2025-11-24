using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Category?> GetOrCreateCategoryAsync(Guid userId, string categoryName, TransactionType type)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            _logger.LogWarning("Attempted to create category with empty name for user {UserId}", userId);
            return null;
        }

        var normalizedName = categoryName.Trim().ToLowerInvariant();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId 
                && c.Name.ToLower() == normalizedName 
                && c.Type == type);

        if (category == null)
        {
            category = new Category
            {
                UserId = userId,
                Name = CapitalizeFirstLetter(normalizedName),
                Type = type
            };
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created new category '{CategoryName}' for user {UserId}", category.Name, userId);
        }

        return category;
    }

    public async Task<List<Category>> GetUserCategoriesAsync(Guid userId, TransactionType? type = null)
    {
        var query = _context.Categories.Where(c => c.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    private static string CapitalizeFirstLetter(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return char.ToUpper(text[0]) + text.Substring(1);
    }
}