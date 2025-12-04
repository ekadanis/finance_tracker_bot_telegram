using System.Globalization;
using System.Text.RegularExpressions;

namespace FinanceTracker.Api.Utils;

public static partial class CommandParser
{
    private static readonly string[] DateFormats =
    {
        "d/M/yyyy",      // 1/1/2025
        "dd/M/yyyy",     // 30/1/2025
        "d/MM/yyyy",     // 1/11/2025
        "dd/MM/yyyy",    // 30/11/2025
        "d-M-yyyy",      // 1-1-2025
        "dd-M-yyyy",     // 30-1-2025
        "d-MM-yyyy",     // 1-11-2025
        "dd-MM-yyyy",    // 30-11-2025
        "d/M/yy",        // 1/1/25
        "dd/M/yy",       // 30/1/25
        "d/MM/yy",       // 1/11/25
        "dd/MM/yy",      // 30/11/25
        "d-M-yy",        // 1-1-25
        "dd-M-yy",       // 30-1-25
        "d-MM-yy",       // 1-11-25
        "dd-MM-yy"       // 30-11-25
    };

    [GeneratedRegex(@"^/([a-zA-Z0-9_-]+)", RegexOptions.Compiled)]
    private static partial Regex CommandRegex();

    [GeneratedRegex(@"^/[a-zA-Z0-9_-]+\s+(.+)", RegexOptions.Compiled)]
    private static partial Regex ArgumentsRegex();

    public static (string? Category, string? Note, decimal? Amount) ParseTransaction(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, null, null);

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            return (null, null, null);

        var command = parts[0];

        if (!command.Equals("/in", StringComparison.Ordinal) && 
            !command.Equals("/out", StringComparison.Ordinal))
            return (null, null, null);

        var category = parts[1];
        var amountText = NormalizeNumber(parts[^1]);
        var note = parts.Length > 3 ? string.Join(" ", parts[2..^1]) : null;

        if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            return (null, null, null);

        if (amount <= 0)
            return (null, null, null);

        return (category, note, amount);
    }

    private static string NormalizeNumber(string value)
    {
        return value
            .Replace("Rp", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".", "")   // Separator ribuan Indonesia
            .Replace(",", "")   // Separator ribuan international
            .Trim();
    }

    /// <summary>
    /// Parse recap command: "/recap [start_date] [end_date]"
    /// Formats supported:
    /// - /recap 01/11/2024 30/11/2024
    /// - /recap 01-11-2024 30-11-2024
    /// - /recap 2024-11-01 2024-11-30
    /// </summary>
    public static (DateOnly? StartDate, DateOnly? EndDate) ParseRecapPeriod(string text)
    {
        Console.WriteLine($"📥 Input text: '{text}'");
        
        if (string.IsNullOrWhiteSpace(text))
            return (null, null);

        text = text.Replace(" - ", " ").Trim();
        
        Console.WriteLine($"🔧 After replace: '{text}'");
        
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        Console.WriteLine($"🔢 Parts count: {parts.Length}");
        for (int i = 0; i < parts.Length; i++)
        {
            Console.WriteLine($"   Part[{i}]: '{parts[i]}'");
        }

        if (parts.Length < 3)
        {
            Console.WriteLine("❌ Not enough parts");
            return (null, null);
        }

        if (!parts[0].Equals("/recap", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("❌ Not a /recap command");
            return (null, null);
        }

        var startText = parts[1];
        var endText = parts[2];
        
        Console.WriteLine($"📅 Parsing startDate: '{startText}'");
        Console.WriteLine($"📅 Parsing endDate: '{endText}'");

        if (!TryParseDate(startText, out var start) || 
            !TryParseDate(endText, out var end))
        {
            Console.WriteLine("❌ Date parsing failed");
            return (null, null);
        }

        if (start > end)
        {
            (start, end) = (end, start);
        }

        Console.WriteLine($"✅ Success! StartDate: {start}, EndDate: {end}");
        return (start, end);
    }

    private static bool TryParseDate(string text, out DateOnly date)
    {
        Console.WriteLine($"🔍 Trying to parse: '{text}'");
        
        foreach (var format in DateFormats)
        {
            if (DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                Console.WriteLine($"✅ Success with format: '{format}' -> {date}");
                return true;
            }
        }

        Console.WriteLine($"❌ Failed to parse: '{text}' with all {DateFormats.Length} formats");
        date = default;
        return false;
    }

    public static string GetCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var match = CommandRegex().Match(text);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public static bool IsCommand(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.StartsWith('/');
    }

    public static string GetArguments(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var match = ArgumentsRegex().Match(text);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}