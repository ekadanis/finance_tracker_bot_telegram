using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Interfaces;

public interface ICategoryService
{
    Task<Category?> GetOrCreateCategoryAsync(Guid userId, string categoryName, TransactionType type);
    Task<List<Category>> GetUserCategoriesAsync(Guid userId, TransactionType? type = null);
}