using FinanceTracker.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Environment);
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddTelegramServices(builder.Configuration);
builder.Services.AddValidationServices();

var app = builder.Build();

app.ConfigureMiddleware(builder.Environment);

await app.MigrateDatabaseAsync();

app.Run();