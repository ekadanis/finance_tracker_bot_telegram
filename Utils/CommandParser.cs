using System.Globalization;

namespace FinanceTracker.Api.Utils;

public class CommandParser
{
    public static (string? Category, string? Note, decimal? Amount) ParseTransaction(string message)
    {
        var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 4) return (null, null, null);

        var category = parts[1];
        var amount = ParseAmount(parts[^1]);
        var note = string.Join(" ", parts[2..^1]);

        return (category, note, amount);
    }

    public static (DateTime? StartDate, DateTime? EndDate) ParseRecap(string message)
    {
        var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 4) return (null, null);

        var startDateStr = parts[1];
        var endDateStr = parts[3];

        var startDate = ParseDate(startDateStr);
        var endDate = ParseDate(endDateStr);

        return (startDate, endDate);
    }

    private static decimal? ParseAmount(string text)
    {
        text = text.Replace(".", "").Replace(",", ".");
        
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    private static DateTime? ParseDate(string text)
    {
        var formats = new[] { "d/M/yyyy", "dd/MM/yyyy", "d/M/yy", "dd/MM/yy" };
        
        if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        return null;
    }
}