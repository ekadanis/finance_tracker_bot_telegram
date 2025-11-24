using FinanceTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(
        this WebApplication app,
        IWebHostEnvironment environment)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        if (environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        app.MapControllers();
        app.MapGet("/health", () => Results.Ok(new { 
            status = "Healthy", 
            timestamp = DateTime.UtcNow 
        }));

        return app;
    }
    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

        const int maxAttempts = 20;
        var delay = TimeSpan.FromSeconds(3);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting database migration (attempt {Attempt}/{MaxAttempts})...", attempt, maxAttempts);
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully!");
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Migration attempt {Attempt} failed", attempt);
                if (attempt == maxAttempts)
                {
                    logger.LogError("All migration attempts failed!");
                    throw;
                }
                await Task.Delay(delay);
            }
        }

        return app;
    }
}