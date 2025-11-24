namespace FinanceTracker.Api.Constants;

public static class ErrorMessages
{
    public const string InvalidIncomeFormat = "❌ Format salah! Gunakan: `/in [kategori] [keterangan] [jumlah]`\nContoh: `/in gaji bulanan 5000000`";
    public const string InvalidExpenseFormat = "❌ Format salah! Gunakan: `/out [kategori] [keterangan] [jumlah]`\nContoh: `/out makan nasi goreng 15000`";
    public const string InvalidRecapFormat = "❌ Format salah! Gunakan: `/recap [tanggal_mulai] -[tanggal_akhir]`\nContoh: `/recap 01/11/2024 30/11/2024`";
    public const string CategoryCreationFailed = "❌ Gagal membuat kategori.";
    public const string NoTransactionsFound = "📊 Tidak ada transaksi di periode ini.";
    public const string GenericError = "❌ Terjadi kesalahan. Silakan coba lagi.";
}