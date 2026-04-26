namespace UONPayApp.Models;

/// <summary>
/// Root object saved to uonpayapp-data.json.
/// This is the whole local database for the demo app.
/// </summary>
public class AppDataStore
{
    // Remembers if the user was logged in when the app last closed.
    public bool IsLoggedIn { get; set; }

    // Profile of the currently logged-in student. Password is not copied here.
    public UserProfile? CurrentUser { get; set; }

    // Approved local login accounts. Each account also owns its saved payment cards.
    public List<LoginAccount> LoginAccounts { get; set; } = [];

    // Old shared history field kept only for compatibility with older saved JSON files.
    // New code uses AccountLedgers[StudentId].Transactions instead.
    public List<TransactionItem> Transactions { get; set; } = [];

    // Main per-account storage. Key = StudentId, value = that student's due amount and history.
    public Dictionary<string, AccountLedger> AccountLedgers { get; set; } = [];
}
