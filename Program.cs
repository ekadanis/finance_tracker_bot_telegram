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
