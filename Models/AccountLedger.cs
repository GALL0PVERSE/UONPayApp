namespace UONPayApp.Models;

/// <summary>
/// One student's separate balance ledger.
/// TotalDue is the starting amount owed; Transactions are that student's payments only.
/// </summary>
public class AccountLedger
{
    public decimal TotalDue { get; set; }
    public List<TransactionItem> Transactions { get; set; } = [];
}
