using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FinanceTracker.Api.Utils;
using FinanceTracker.Api.Models;
using System.Text.Json;

namespace FinanceTracker.Api.Services;

public class TelegramService : ITelegramService 
{
    private readonly string _botToken;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TelegramService> _logger;
    private readonly HttpClient _httpClient;

    public TelegramService(
        ITelegramBotClient botClient,
        IConfiguration configuration,
        IUserService userService,
        ICategoryService categoryService,
        ITransactionService transactionService,
        ILogger<TelegramService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _botToken = configuration["TelegramSettings:BotToken"] ?? throw new ArgumentNullException("BotToken");
        _userService = userService;
        _categoryService = categoryService;
        _transactionService = transactionService;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try 
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send message. Response: {Response}", responseBody);
                throw new Exception($"Telegram API error: {responseBody}");
            }
            
            _logger.LogInformation("Message sent to chat {ChatId}", chatId);
        } 
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Failed to send message to chat {ChatId}. Message: {Message}", chatId, message);
            throw;
        }
    }

    public async Task ProcessUpdateAsync(Update update)
    {
        _logger.LogInformation("ProcessUpdateAsync called. Update Type: {Type}", update.Type);
        
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
        {
            _logger.LogWarning("Update ignored. Type: {Type}, HasMessage: {HasMessage}, HasText: {HasText}", 
                update.Type, 
                update.Message != null, 
                update.Message?.Text != null);
            return;
        }

        var message = update.Message;
        var chatId = message.Chat.Id;
        var telegramId = message.From?.Id ?? 0;
        var text = message.Text;
        var username = message.From?.Username ?? $"User_{telegramId}";

        _logger.LogInformation("Processing message from {Username} ({TelegramId}): {Text}", 
            username, telegramId, text);

        try
        {
            var user = await _userService.GetOrCreateUserAsync(telegramId, username);

            if (text.StartsWith("/start"))
            {
                await HandleStartCommandAsync(chatId, username);
            }
            else if (text.StartsWith("/help"))
            {
                await HandleHelpCommandAsync(chatId);
            }
            else if (text.StartsWith("/in"))
            {
                await HandleIncomeCommandAsync(chatId, text, user.Id);
            }
            else if (text.StartsWith("/out "))
            {
                await HandleExpenseCommandAsync(chatId, text, user.Id);
            }
            else if (text.StartsWith("/saldo"))
            {
                await HandleBalanceCommandAsync(chatId, user.Id);
            }
            else if (text.StartsWith("/recap "))
            {
                await HandleRecapCommandAsync(chatId, text, user.Id);
            }
            else if (text.StartsWith("/categories"))
            {
                await HandleCategoriesCommandAsync(chatId, user.Id);
            }
            else
            {
                await SendMessageAsync(chatId, "❌ Perintah tidak dikenal. Ketik /help untuk melihat daftar perintah.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update from chat {ChatId}", chatId);
            try
            {
                await SendMessageAsync(chatId, "❌ Terjadi kesalahan. Silakan coba lagi.");
            }
            catch (Exception sendEx)
            {
                _logger.LogError(sendEx, "Failed to send error message to chat {ChatId}", chatId);
            }
        }
    }

    private async Task HandleIncomeCommandAsync(long chatId, string text, Guid userId)
    {
        _logger.LogInformation("HandleIncomeCommandAsync: {Text}", text);
        
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/in [kategori] [keterangan] [jumlah]`\nContoh: `/in gaji bulanan 5000000`");
            return;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Income);
        if (categoryEntity == null)
        {
            await SendMessageAsync(chatId, "❌ Gagal membuat kategori.");
            return;
        }

        await _transactionService.AddTransactionAsync(
            userId, 
            categoryEntity.Id, 
            TransactionType.Income, 
            amount.Value, 
            note ?? "", 
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        await SendMessageAsync(chatId, $"✅ *Pemasukan dicatat!*\n\n💰 Kategori: {category}\n📝 Keterangan: {note}\n💵 Jumlah: Rp{amount:N0}");
    }

    private async Task HandleExpenseCommandAsync(long chatId, string text, Guid userId)
    {
        _logger.LogInformation("HandleExpenseCommandAsync: {Text}", text);
        
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/out [kategori] [keterangan] [jumlah]`\nContoh: `/out makan nasi goreng 15000`");
            return;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Expense);
        if (categoryEntity == null)
        {
            await SendMessageAsync(chatId, "❌ Gagal membuat kategori.");
            return;
        }

        await _transactionService.AddTransactionAsync(
            userId, 
            categoryEntity.Id, 
            TransactionType.Expense, 
            amount.Value, 
            note ?? "", 
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        await SendMessageAsync(chatId, $"✅ *Pengeluaran dicatat!*\n\n💸 Kategori: {category}\n📝 Keterangan: {note}\n💵 Jumlah: Rp{amount:N0}");
    }

    private async Task HandleBalanceCommandAsync(long chatId, Guid userId)
    {
        _logger.LogInformation("HandleBalanceCommandAsync for user {UserId}", userId);
        
        var balance = await _transactionService.GetBalanceAsync(userId);
        var (income, expense) = await _transactionService.GetTotalIncomeExpenseAsync(userId);

        var message = $"💰 *Saldo Kamu*\n\n" +
                     $"📈 Total Pemasukan: Rp{income:N0}\n" +
                     $"📉 Total Pengeluaran: Rp{expense:N0}\n" +
                     $"━━━━━━━━━━━━━━\n" +
                     $"💵 *Saldo: Rp{balance:N0}*";

        await SendMessageAsync(chatId, message);
    }

    private async Task HandleRecapCommandAsync(long chatId, string text, Guid userId)
    {
        _logger.LogInformation("HandleRecapCommandAsync: {Text}", text);
        
        var (startDate, endDate) = CommandParser.ParseRecap(text);

        if (startDate == null || endDate == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/recap [tanggal1] - [tanggal2]`\nContoh: `/recap 1/10/2025 - 30/10/2025`");
            return;
        }

        // Gunakan GetTransactionsByPeriodAsync untuk dapat detail lengkap
        var transactions = await _transactionService.GetTransactionsByPeriodAsync(
            userId, 
            DateOnly.FromDateTime(startDate.Value), 
            DateOnly.FromDateTime(endDate.Value)
        );

        if (transactions.Count == 0)
        {
            await SendMessageAsync(chatId, $"📊 *Rekap Periode*\n📅 {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\n\n❌ Tidak ada transaksi dalam periode ini.");
            return;
        }

        // Hitung total dari transactions
        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var balance = income - expense;

        // Format message dengan detail transaksi
        var message = $"📊 *Rekap Periode*\n" +
                     $"📅 {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\n\n";

        // Tampilkan Income
        var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();
        if (incomeTransactions.Any())
        {
            message += "📈 *PEMASUKAN*\n";
            foreach (var t in incomeTransactions)
            {
                message += $"• {t.Date:dd/MM} - {t.Category.Name}: Rp{t.Amount:N0} ";
                if (!string.IsNullOrEmpty(t.Note))
                    message += $"  _{ t.Note }_\n";
            }
            message += "\n";
        }

        // Tampilkan Expense
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        if (expenseTransactions.Any())
        {
            message += "📉 *PENGELUARAN*\n";
            foreach (var t in expenseTransactions)
            {
                message += $"• {t.Date:dd/MM} - {t.Category.Name}: Rp{t.Amount:N0} ";
                if (!string.IsNullOrEmpty(t.Note))
                    message += $"  _{ t.Note }_\n";
            }
            message += "\n";
        }

        // Summary
        message += "━━━━━━━━━━━━━━\n" +
                  $"📈 Total Pemasukan: Rp{income:N0}\n" +
                  $"📉 Total Pengeluaran: Rp{expense:N0}\n" +
                  $"💵 *Saldo: Rp{balance:N0}*";

        await SendMessageAsync(chatId, message);
    }

    private async Task HandleStartCommandAsync(long chatId, string username)
    {
        _logger.LogInformation("HandleStartCommandAsync for {Username}", username);
        
        var message = $"👋 Halo *{username}*! Selamat datang di Finance Tracker Bot!\n\n" +
                     "📝 *Command yang tersedia:*\n" +
                     "`/in [kategori] [keterangan] [jumlah]` - Catat pemasukan\n" +
                     "`/out [kategori] [keterangan] [jumlah]` - Catat pengeluaran\n" +
                     "`/saldo` - Lihat saldo terkini\n" +
                     "`/recap [tgl1] - [tgl2]` - Rekap periode\n" +
                     "`/categories` - Lihat semua kategori\n" +
                     "`/help` - Panduan lengkap\n\n" +
                     "Contoh:\n" +
                     "`/in gaji bulanan 5000000`\n" +
                     "`/out makan nasi goreng 15000`\n" +
                     "`/recap 1/11/2025 - 17/11/2025`";

        await SendMessageAsync(chatId, message);
    }

    private async Task HandleHelpCommandAsync(long chatId)
    {
        var helpMessage = @"📖 *Panduan Penggunaan Bot*

            💰 *TRANSAKSI*
            /in [kategori] [keterangan] [jumlah]
            Catat pemasukan
            Contoh: `/in gaji bulanan 5000000`

            /out [kategori] [keterangan] [jumlah]
            Catat pengeluaran
            Contoh: `/out makan nasgor 15000`

            📊 *LAPORAN*
            /saldo
            Lihat saldo total kamu

            /recap [tanggal1] - [tanggal2]
            Lihat rekap periode tertentu
            Contoh: 
            `/recap 1/11/2025 - 17/11/2025`

            /bulanan
            Lihat rekap bulan ini

            /categories
            Lihat semua kategori kamu

            /help
            Tampilkan panduan ini";

        await SendMessageAsync(chatId, helpMessage);
    }

    private async Task HandleCategoriesCommandAsync(long chatId, Guid userId)
    {
        _logger.LogInformation("HandleCategoriesCommandAsync for user {UserId}", userId);
        
        var incomeCategories = await _categoryService.GetUserCategoriesAsync(userId, TransactionType.Income);
        var expenseCategories = await _categoryService.GetUserCategoriesAsync(userId, TransactionType.Expense);

        var message = "📂 *Kategori Kamu*\n\n";

        if (incomeCategories.Any())
        {
            message += "📈 *Pemasukan:*\n";
            foreach (var cat in incomeCategories)
            {
                message += $"• {cat.Name}\n";
            }
            message += "\n";
        }

        if (expenseCategories.Any())
        {
            message += "📉 *Pengeluaran:*\n";
            foreach (var cat in expenseCategories)
            {
                message += $"• {cat.Name}\n";
            }
        }

        if (!incomeCategories.Any() && !expenseCategories.Any())
        {
            message += "❌ Belum ada kategori. Buat transaksi pertama kamu!";
        }

        await SendMessageAsync(chatId, message);
    }
}