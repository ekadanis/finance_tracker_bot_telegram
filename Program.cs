using FluentValidation;
using FluentValidation.AspNetCore;
using Telegram.Bot;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

string? botToken = Environment.GetEnvironmentVariable("TelegramSettings__BotToken")
    ?? builder.Configuration["TelegramSettings:BotToken"];

string? webhookUrl = Environment.GetEnvironmentVariable("TelegramSettings__WebhookUrl")
    ?? builder.Configuration["TelegramSettings:WebhookUrl"];

string? dbConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(botToken))
{
    Console.Error.WriteLine("❌ ERROR: TelegramSettings__BotToken is not configured!");
    Console.Error.WriteLine("   Set in appsettings.json or environment variable");
    throw new InvalidOperationException("Telegram bot token is required");
}

if (string.IsNullOrWhiteSpace(dbConnection))
{
    Console.Error.WriteLine("❌ ERROR: Database connection string is not configured!");
    throw new InvalidOperationException("Database connection string is required");
}

Console.WriteLine($"✅ Bot Token: {botToken[..15]}... (length: {botToken.Length})");
Console.WriteLine($"✅ Webhook URL: {webhookUrl ?? "(not set)"}");
Console.WriteLine($"✅ Database: {(dbConnection.Contains("localhost") ? "Local PostgreSQL" : "Docker PostgreSQL")}");

var telegramClient = new TelegramBotClient(botToken);
builder.Services.AddSingleton<ITelegramBotClient>(telegramClient);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnection));

builder.Services.AddHttpClient();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITelegramService, TelegramService>();

if (builder.Environment.IsProduction())
{
    builder.Services.AddHostedService<SchedulerService>();
    Console.WriteLine("✅ SchedulerService registered (Production mode)");
}
else
{
    Console.WriteLine("⚠️  SchedulerService disabled (Development mode)");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger di semua environment (untuk debugging production)
app.UseSwagger();
app.UseSwaggerUI();
Console.WriteLine($"✅ Swagger enabled: {(app.Environment.IsDevelopment() ? "http://localhost:5222" : "http://localhost:8081")}/swagger");

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    const int maxAttempts = 20;
    var delay = TimeSpan.FromSeconds(3);

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            Console.WriteLine($"🔄 Attempting database migration (attempt {attempt}/{maxAttempts})...");
            db.Database.Migrate();
            Console.WriteLine("✅ Database migrations applied successfully!");
            break;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"⚠️  Migration attempt {attempt} failed: {ex.Message}");
            if (attempt == maxAttempts)
            {
                Console.Error.WriteLine("❌ All migration attempts failed!");
                throw;
            }
            Thread.Sleep(delay);
        }
    }
}

app.MapGet("/health", () => Results.Ok("Healthy"));

Console.WriteLine("🚀 Starting web host...");
app.Run();

