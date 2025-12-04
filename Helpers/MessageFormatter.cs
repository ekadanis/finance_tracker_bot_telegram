using FinanceTracker.Api.Models;
using System.Text;

namespace FinanceTracker.Api.Helpers;

public static class MessageFormatter
{
    public static string FormatTransactionSuccess(string type, string category, string? note, decimal amount)
    {
        return $"✅ *{type} dicatat!*\n\n" +
               $"💰 Kategori: {category}\n" +
               $"📝 Keterangan: {note ?? "-"}\n" +
               $"💵 Jumlah: Rp{amount:N0}";
    }

    public static string FormatBalance(decimal balance, decimal income, decimal expense)
    {
        return $"💰 *Saldo Kamu*\n\n" +
               $"📈 Total Pemasukan: Rp{income:N0}\n" +
               $"📉 Total Pengeluaran: Rp{expense:N0}\n" +
               $"━━━━━━━━━━━━━━\n" +
               $"💵 *Saldo: Rp{balance:N0}*";
    }

    public static string FormatRecap(
        DateOnly startDate, 
        DateOnly endDate, 
        decimal income, 
        decimal expense, 
        decimal balance, 
        List<Transaction> transactions)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("📊 *REKAP KEUANGAN*");
        sb.AppendLine($"📅 {startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}");
        sb.AppendLine();

        var incomes = transactions.Where(t => t.Type == TransactionType.Income)
                                  .OrderByDescending(x => x.Date)
                                  .ToList();
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense)
                                   .OrderByDescending(x => x.Date)
                                   .ToList();

        // PEMASUKAN
        if (incomes.Any())
        {
            sb.AppendLine("💰 *PEMASUKAN*");
            foreach (var t in incomes)
            {
                var note = string.IsNullOrWhiteSpace(t.Note) ? "" : $" - _{t.Note}_";
                sb.AppendLine($"{t.Date:dd/MM} • {t.Category.Name}: `Rp {t.Amount:N0}`{note}");
            }
            sb.AppendLine();
        }

        // PENGELUARAN
        if (expenses.Any())
        {
            sb.AppendLine("💸 *PENGELUARAN*");
            foreach (var t in expenses)
            {
                var note = string.IsNullOrWhiteSpace(t.Note) ? "" : $" - _{t.Note}_";
                sb.AppendLine($"{t.Date:dd/MM} • {t.Category.Name}: `Rp {t.Amount:N0}`{note}");
            }
            sb.AppendLine();
        }

        // Summary
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"📈 Total Pemasukan:  `Rp {income:N0}`");
        sb.AppendLine($"📉 Total Pengeluaran: `Rp {expense:N0}`");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
        
        var balanceIcon = balance >= 0 ? "✅" : "⚠️";
        sb.AppendLine($"{balanceIcon} *Saldo Akhir: Rp {balance:N0}*");

        return sb.ToString();
    }

    public static string FormatCategories(List<Category> incomeCategories, List<Category> expenseCategories)
    {
        var message = "📂 *Kategori Kamu*\n\n";

        if (incomeCategories.Any())
        {
            message += "📈 *PEMASUKAN*\n";
            foreach (var cat in incomeCategories)
            {
                message += $"• {cat.Name}\n";
            }
            message += "\n";
        }

        if (expenseCategories.Any())
        {
            message += "📉 *PENGELUARAN*\n";
            foreach (var cat in expenseCategories)
            {
                message += $"• {cat.Name}\n";
            }
        }

        if (!incomeCategories.Any() && !expenseCategories.Any())
        {
            message = "📂 Kamu belum memiliki kategori.\nKategori akan otomatis dibuat saat kamu mencatat transaksi.";
        }

        return message;
    }

    public static string GetHelpMessage()
    {
        return "🤖 *Perintah yang tersedia:*\n\n" +
               "💰 `/in [kategori] [keterangan] [jumlah]` - Catat pemasukan\n" +
               "   Contoh: `/in gaji bulanan 5000000`\n\n" +
               "💸 `/out [kategori] [keterangan] [jumlah]` - Catat pengeluaran\n" +
               "   Contoh: `/out makan nasi goreng 15000`\n\n" +
               "📊 `/saldo` - Lihat saldo kamu\n\n" +
               "📅 `/recap [tanggal_mulai] [tanggal_akhir]` - Lihat rekap periode\n" +
               "   Contoh: `/recap 01/11/2024 30/11/2024`\n\n" +
               "📂 `/categories` - Lihat semua kategori\n\n" +
               "❓ `/help` - Tampilkan pesan ini";
    }

    public static string GetWelcomeMessage(string username)
    {
        return $"👋 Halo {username}!\n\n" +
               "Selamat datang di *Finance Tracker Bot*! 🎉\n\n" +
               "Gunakan `/help` untuk melihat perintah yang tersedia.";
    }

    public static string GetInvalidFormatMessage()
    {
        return "❌ Format perintah tidak dikenali.\n\n" +
               "Gunakan `/help` untuk melihat daftar perintah.";
    }

    public static string GetErrorMessage()
    {
        return "❌ Terjadi kesalahan. Silakan coba lagi.";
    }

    private static string FormatTransactionLine(Transaction t)
    {
        var line = $"• {t.Date:dd/MM} - {t.Category.Name}: Rp{t.Amount:N0}";
        if (!string.IsNullOrEmpty(t.Note))
            line += $"\n  _{t.Note}_\n";
        else
            line += "\n";
        return line;
    }
}