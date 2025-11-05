using Telegram.Bot;
using FinanceTracker.Api.Data;
using FinanceTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

var dbConnection = builder.Configuration["DatabaseSettings:ConnectionString"];
if (dbConnection != null && dbConnection.StartsWith("env:"))
{
    var envKey = dbConnection.Replace("env:", "");
    dbConnection = Environment.GetEnvironmentVariable(envKey);
}

var botToken = builder.Configuration["TelegramSettings:BotToken"];
if (botToken != null && botToken.StartsWith("env:"))
{
    var envKey = botToken.Replace("env:", "");
    botToken = Environment.GetEnvironmentVariable(envKey);
}

// fallback ke single-underscore env var jika belum ada
if (string.IsNullOrWhiteSpace(botToken))
{
    var alt = Environment.GetEnvironmentVariable("TelegramSettings_BotToken");
    if (!string.IsNullOrWhiteSpace(alt))
    {
        botToken = alt;
    }
}

// pengecekan tegas
if (string.IsNullOrWhiteSpace(botToken) || botToken == "your_bot_token")
{
    Console.Error.WriteLine("ERROR: Telegram bot token tidak dikonfigurasi atau masih placeholder.\n" +
                            "Set environment variable TelegramSettings__BotToken atau TelegramSettings_BotToken,\n" +
                            "atau gunakan dotnet user-secrets. Contoh (bash):\n" +
                            "  export TelegramSettings_BotToken=\"123456789:ABC...\" && dotnet run --environment Development");
    return;
}

var token = botToken!;

// DEBUG: tunjukkan panjang token (tidak menampilkan token)
Console.WriteLine($"[DBG] Telegram token length: {token.Length}");

// buat dan register TelegramBotClient
var telegramClient = new TelegramBotClient(token);
builder.Services.AddSingleton<TelegramBotClient>(telegramClient);
builder.Services.AddSingleton<ITelegramBotClient>(sp => sp.GetRequiredService<TelegramBotClient>());

if (!string.IsNullOrWhiteSpace(dbConnection))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConnection));
}
else
{
    // jika belum ada connection string, Anda bisa menggunakan InMemory untuk dev/test:
    // builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("dev-db"));
}

// register app services (sesuaikan namespace jika beda)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITelegramService, TelegramService>();

// services API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Ini yang membuat proses tetap berjalan dan mendengarkan
Console.WriteLine("[INFO] Starting web host...");
app.Run();