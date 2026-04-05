namespace UONPayApp.Models;

public class AccountLedger
{
    public decimal TotalDue { get; set; }
    public List<TransactionItem> Transactions { get; set; } = [];
}
