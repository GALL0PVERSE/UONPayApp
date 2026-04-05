namespace UONPayApp.Models;

public class AppDataStore
{
    public bool IsLoggedIn { get; set; }
    public UserProfile? CurrentUser { get; set; }
    public List<LoginAccount> LoginAccounts { get; set; } = [];
    public List<TransactionItem> Transactions { get; set; } = [];
    public Dictionary<string, AccountLedger> AccountLedgers { get; set; } = [];
}
