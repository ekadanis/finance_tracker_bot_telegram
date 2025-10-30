using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services;

public interface ICategoryService
{
    Task<Category?> GetOrCreateCategoryAsync(Guid userId, string categoryName, TransactionType type);
    Task<List<Category>> GetUserCategoriesAsync(Guid userId, TransactionType? type = null);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetOrCreateCategoryAsync(Guid userId, string categoryName, TransactionType type)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == categoryName && c.Type == type);

        if (category == null)
        {
            category = new Category
            {
                UserId = userId,
                Name = categoryName,
                Type = type
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
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
}