using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;
using FinanceTracker.Api.Utils;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Services;

public class TelegramService : ITelegramService {
    private readonly TelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        TelegramBotClient botClient,
        IUserService userService,
        ICategoryService categoryService,
        ITransactionService transactionService,
        ILogger<TelegramService> logger)
    {
        _botClient = botClient;
        _userService = userService;
        _categoryService = categoryService;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try {
            await _botClient.SendMessage(new ChatId(chatId), message, parseMode: ParseMode.Markdown, cancellationToken: default);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            throw;
        }
    }


    public async Task ProcessUpdateAsync(Update update)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text;
        var username = message.From?.Username ?? "Unknown";

        try
        {
            var user = await _userService.GetOrCreateUserAsync(chatId, username);

            if (text.StartsWith("/in "))
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
            else if (text.StartsWith("/start"))
            {
                await HandleStartCommandAsync(chatId, username);
            }
            else
            {
                await SendMessageAsync(chatId, "❌ Command tidak dikenal. Gunakan /start untuk bantuan.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update from chat {ChatId}", chatId);
            await SendMessageAsync(chatId, "❌ Terjadi kesalahan. Silakan coba lagi.");
        }
    }

    private async Task HandleIncomeCommandAsync(long chatId, string text, Guid userId)
    {
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/in [kategori] [keterangan] [jumlah]`\nContoh: `/in gaji bulanan 5000000`");
            return;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Income);
        await _transactionService.AddTransactionAsync(userId, categoryEntity!.Id, TransactionType.Income, amount.Value, note ?? "", DateTime.UtcNow);

        await SendMessageAsync(chatId, $"✅ *Pemasukan dicatat!*\n\n💰 Kategori: {category}\n📝 Keterangan: {note}\n💵 Jumlah: Rp{amount:N0}");
    }

    private async Task HandleExpenseCommandAsync(long chatId, string text, Guid userId)
    {
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/out [kategori] [keterangan] [jumlah]`\nContoh: `/out makan nasi goreng 15000`");
            return;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Expense);
        await _transactionService.AddTransactionAsync(userId, categoryEntity!.Id, TransactionType.Expense, amount.Value, note ?? "", DateTime.UtcNow);

        await SendMessageAsync(chatId, $"✅ *Pengeluaran dicatat!*\n\n💸 Kategori: {category}\n📝 Keterangan: {note}\n💵 Jumlah: Rp{amount:N0}");
    }

    private async Task HandleBalanceCommandAsync(long chatId, Guid userId)
    {
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
        var (startDate, endDate) = CommandParser.ParseRecap(text);

        if (startDate == null || endDate == null)
        {
            await SendMessageAsync(chatId, "❌ Format salah! Gunakan: `/recap [tanggal1] - [tanggal2]`\nContoh: `/recap 1/10/2025 - 30/10/2025`");
            return;
        }

        var (income, expense, balance) = await _transactionService.GetRecapAsync(userId, startDate.Value, endDate.Value);

        var message = $"📊 *Rekap Periode*\n" +
                     $"📅 {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}\n\n" +
                     $"📈 Total Pemasukan: Rp{income:N0}\n" +
                     $"📉 Total Pengeluaran: Rp{expense:N0}\n" +
                     $"━━━━━━━━━━━━━━\n" +
                     $"💵 *Saldo: Rp{balance:N0}*";

        await SendMessageAsync(chatId, message);
    }

    private async Task HandleStartCommandAsync(long chatId, string username)
    {
        var message = $"👋 Halo *{username}*! Selamat datang di Finance Tracker Bot!\n\n" +
                     "📝 *Command yang tersedia:*\n" +
                     "`/in [kategori] [keterangan] [jumlah]` - Catat pemasukan\n" +
                     "`/out [kategori] [keterangan] [jumlah]` - Catat pengeluaran\n" +
                     "`/saldo` - Lihat saldo\n" +
                     "`/recap [tgl1] - [tgl2]` - Rekap periode\n\n" +
                     "Contoh:\n" +
                     "`/in gaji bulanan 5000000`\n" +
                     "`/out makan nasi goreng 15000`\n" +
                     "`/recap 1/10/2025 - 30/10/2025`";

        await SendMessageAsync(chatId, message);
    }
}