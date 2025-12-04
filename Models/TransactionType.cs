using System.Text.Json.Serialization;

namespace FinanceTracker.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Income = 0,
    Expense = 1
}