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
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Category?> GetOrCreateCategoryAsync(Guid userId, string categoryName, TransactionType type)
    {
        // Validasi categoryName tidak boleh kosong
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            _logger.LogWarning("Attempted to create category with empty name for user {UserId}", userId);
            return null; // Return null jika category name invalid
        }

        // Trim & normalize category name (lowercase untuk case-insensitive)
        categoryName = categoryName.Trim();
        var normalizedName = categoryName.ToLower();

        // Cari category dengan case-insensitive comparison
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId 
                && c.Name.ToLower() == normalizedName 
                && c.Type == type);

        if (category == null)
        {
            // Simpan dengan format Capitalized (huruf pertama besar)
            var formattedName = char.ToUpper(normalizedName[0]) + normalizedName.Substring(1);
            
            category = new Category
            {
                UserId = userId,
                Name = formattedName,
                Type = type
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created new category '{CategoryName}' for user {UserId}", formattedName, userId);
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