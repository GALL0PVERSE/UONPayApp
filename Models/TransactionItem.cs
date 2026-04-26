namespace UONPayApp.Models;

/// <summary>
/// One payment record shown in the bill/payment history page.
/// </summary>
public class TransactionItem
{
    public string DateText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
