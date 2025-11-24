using FluentValidation;
using FluentValidation.AspNetCore;
using Telegram.Bot;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Services.CommandHandlers;
using FinanceTracker.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpClient();

        // Add business services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITelegramService, TelegramService>();

        // Register Command Handlers
        services.AddScoped<ICommandHandler, StartCommandHandler>();
        services.AddScoped<ICommandHandler, HelpCommandHandler>();
        services.AddScoped<ICommandHandler, IncomeCommandHandler>();
        services.AddScoped<ICommandHandler, ExpenseCommandHandler>();
        services.AddScoped<ICommandHandler, BalanceCommandHandler>();
        services.AddScoped<ICommandHandler, RecapCommandHandler>();
        services.AddScoped<ICommandHandler, CategoriesCommandHandler>();

        // Add background services only in production
        if (environment.IsProduction())
        {
            services.AddHostedService<SchedulerService>();
        }

        return services;
    }

    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is required");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddTelegramServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var botToken = configuration["TelegramSettings:BotToken"]
            ?? throw new InvalidOperationException("Telegram bot token is required");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

        return services;
    }

    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}