using System.Text.Json;
using UONPayApp.Models;

namespace UONPayApp.Services;

/// <summary>
/// AppDataService is the small local "backend" for this demo app.
/// It stores login accounts, the current logged-in user, each student's balance ledger,
/// payment history, and saved cards in one JSON file on the device.
///
/// Android/Windows local storage path:
/// FileSystem.Current.AppDataDirectory/uonpayapp-data.json
///
/// Important design:
/// - LoginAccounts contains the approved student accounts and their saved cards.
/// - AccountLedgers is a Dictionary keyed by StudentId.
/// - Because the ledger key is the StudentId, every student has separate balance and bills.
/// </summary>
public class AppDataService
{
    // Full local file path where the JSON database is saved on the device.
    private readonly string _filePath;

    // Pretty JSON makes the saved data easier to inspect while testing.
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Store is the whole app database currently loaded in memory.
    public AppDataStore Store { get; private set; } = new();

    public AppDataService()
    {
        // AppDataDirectory is managed by .NET MAUI and points to app-private storage.
        // On Android, this is inside the app sandbox, so other apps cannot normally read it.
        _filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "uonpayapp-data.json");

        // Load existing saved data first. If no file exists, Load() creates an empty store.
        Load();

        // Add default test accounts/ledgers/cards only when the store is empty.
        SeedDefaults();

        // Save immediately so the JSON file exists after first launch.
        Save();
    }

    /// <summary>
    /// Reads the JSON database from local storage into memory.
    /// If the file does not exist or is damaged, the app safely starts with a new empty store.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            Store = new AppDataStore();
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            Store = JsonSerializer.Deserialize<AppDataStore>(json, _jsonOptions) ?? new AppDataStore();
        }
        catch
        {
            // If JSON cannot be read, avoid crashing the app.
            // In a real production app, you would log this error.
            Store = new AppDataStore();
        }
    }

    /// <summary>
    /// Writes the current in-memory database back to the local JSON file.
    /// Call this after login/logout or after adding a payment transaction.
    /// </summary>
    public void Save()
    {
        var json = JsonSerializer.Serialize(Store, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// Checks the entered student ID/email and password against the approved local accounts.
    /// Only accounts listed in Store.LoginAccounts can log in.
    /// </summary>
    public bool TryLogin(string studentIdOrEmail, string password)
    {
        var loginValue = studentIdOrEmail.Trim();
        var passwordValue = password.Trim();

        var matchedAccount = Store.LoginAccounts.FirstOrDefault(account =>
            (string.Equals(account.StudentId, loginValue, StringComparison.OrdinalIgnoreCase)
             || string.Equals(account.Email, loginValue, StringComparison.OrdinalIgnoreCase))
            && string.Equals(account.Password, passwordValue, StringComparison.Ordinal));

        if (matchedAccount is null)
        {
            return false;
        }

        // Make sure this student has their own separate ledger before entering the app.
        EnsureLedgerExists(matchedAccount.StudentId);

        // Save only the profile information as CurrentUser, not the password.
        Store.IsLoggedIn = true;
        Store.CurrentUser = UserProfile.FromLoginAccount(matchedAccount);
        Save();
        return true;
    }

    /// <summary>
    /// Clears the current login state. The local accounts, cards, balances, and histories remain saved.
    /// </summary>
    public void Logout()
    {
        Store.IsLoggedIn = false;
        Store.CurrentUser = null;
        Save();
    }

    /// <summary>
    /// Calculates the current student's outstanding balance.
    /// Each student has a separate TotalDue and separate Transactions list.
    /// </summary>
    public decimal GetOutstandingBalance()
    {
        var ledger = GetCurrentLedger();
        var totalPaid = ledger.Transactions.Sum(x => x.Amount);
        var balance = ledger.TotalDue - totalPaid;
        return balance < 0 ? 0 : balance;
    }

    /// <summary>
    /// Returns only the logged-in student's payment history, newest first.
    /// </summary>
    public List<TransactionItem> GetCurrentTransactions()
    {
        return GetCurrentLedger()
            .Transactions
            .OrderByDescending(x => ParseDate(x.DateText))
            .ToList();
    }

    /// <summary>
    /// Returns only the logged-in student's saved payment cards.
    /// Cards are stored under LoginAccount, so each student can have different cards.
    /// </summary>
    public List<PaymentCard> GetCurrentPaymentCards()
    {
        var studentId = Store.CurrentUser?.StudentId;
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return [];
        }

        var account = Store.LoginAccounts.FirstOrDefault(x => x.StudentId == studentId);
        return account?.PaymentCards ?? [];
    }

    /// <summary>
    /// Adds a payment transaction to the logged-in student's own ledger only.
    /// This is the key function that keeps bills/history separate between accounts.
    /// </summary>
    public void AddTransaction(string title, decimal amount, string paymentMethod, string reference)
    {
        var ledger = GetCurrentLedger();

        ledger.Transactions.Insert(0, new TransactionItem
        {
            DateText = DateTime.Now.ToString("dd MMM yyyy"),
            Title = title,
            Amount = amount,
            Status = "Paid",
            PaymentMethod = paymentMethod,
            Reference = reference
        });

        Save();
    }

    /// <summary>
    /// Finds the ledger for the currently logged-in student.
    /// The dictionary key is the StudentId, for example AccountLedgers["S1234567"].
    /// </summary>
    private AccountLedger GetCurrentLedger()
    {
        var studentId = Store.CurrentUser?.StudentId;
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return new AccountLedger();
        }

        EnsureLedgerExists(studentId);
        return Store.AccountLedgers[studentId];
    }

    /// <summary>
    /// Creates an empty ledger if a student does not have one yet.
    /// This prevents errors when adding new accounts later.
    /// </summary>
    private void EnsureLedgerExists(string studentId)
    {
        if (Store.AccountLedgers.ContainsKey(studentId))
        {
            return;
        }

        Store.AccountLedgers[studentId] = new AccountLedger();
    }

    /// <summary>
    /// Seeds test data for three student accounts.
    /// This only runs when the lists are empty, so it does not overwrite saved user payments.
    /// </summary>
    private void SeedDefaults()
    {
        if (Store.LoginAccounts.Count == 0)
        {
            Store.LoginAccounts =
            [
                new()
                {
                    FullName = "Yuchi Qian",
                    StudentId = "S1234567",
                    Email = "yuchi.qian@uon.edu",
                    Password = "Pass@123",
                    PaymentCards =
                    [
                        new() { CardholderName = "Yuchi Qian", Brand = "Visa", Last4Digits = "4242", Expiry = "09/28" },
                        new() { CardholderName = "Yuchi Qian", Brand = "Mastercard", Last4Digits = "1088", Expiry = "01/29" }
                    ]
                },
                new()
                {
                    FullName = "Alex Tan",
                    StudentId = "S2345678",
                    Email = "alex.tan@uon.edu",
                    Password = "Welcome1",
                    PaymentCards =
                    [
                        new() { CardholderName = "Alex Tan", Brand = "Visa", Last4Digits = "6731", Expiry = "12/27" },
                        new() { CardholderName = "Alex Tan", Brand = "Amex", Last4Digits = "3005", Expiry = "06/29" }
                    ]
                },
                new()
                {
                    FullName = "Mia Lee",
                    StudentId = "S3456789",
                    Email = "mia.lee@uon.edu",
                    Password = "Secure99",
                    PaymentCards =
                    [
                        new() { CardholderName = "Mia Lee", Brand = "Mastercard", Last4Digits = "8844", Expiry = "03/30" },
                        new() { CardholderName = "Mia Lee", Brand = "Visa", Last4Digits = "5512", Expiry = "11/28" }
                    ]
                }
            ];
        }

        if (Store.AccountLedgers.Count == 0)
        {
            // Each dictionary entry is one student's separate balance and history.
            Store.AccountLedgers = new Dictionary<string, AccountLedger>
            {
                ["S1234567"] = new AccountLedger
                {
                    TotalDue = 6200.00m,
                    Transactions =
                    [
                        new() { DateText = DateTime.Now.AddDays(-10).ToString("dd MMM yyyy"), Title = "Tuition Fee", Amount = 1200.00m, Status = "Paid", PaymentMethod = "Visa", Reference = "YQ-1001" },
                        new() { DateText = DateTime.Now.AddDays(-6).ToString("dd MMM yyyy"), Title = "Accommodation", Amount = 800.00m, Status = "Paid", PaymentMethod = "Bank Transfer", Reference = "YQ-1002" },
                        new() { DateText = DateTime.Now.AddDays(-2).ToString("dd MMM yyyy"), Title = "Library Fine", Amount = 50.00m, Status = "Paid", PaymentMethod = "Mastercard", Reference = "YQ-1003" }
                    ]
                },
                ["S2345678"] = new AccountLedger
                {
                    TotalDue = 4500.00m,
                    Transactions =
                    [
                        new() { DateText = DateTime.Now.AddDays(-12).ToString("dd MMM yyyy"), Title = "Tuition Fee", Amount = 900.00m, Status = "Paid", PaymentMethod = "Visa", Reference = "AT-2001" },
                        new() { DateText = DateTime.Now.AddDays(-4).ToString("dd MMM yyyy"), Title = "Lab Fee", Amount = 250.00m, Status = "Paid", PaymentMethod = "Bank Transfer", Reference = "AT-2002" }
                    ]
                },
                ["S3456789"] = new AccountLedger
                {
                    TotalDue = 5800.00m,
                    Transactions =
                    [
                        new() { DateText = DateTime.Now.AddDays(-15).ToString("dd MMM yyyy"), Title = "Accommodation", Amount = 1500.00m, Status = "Paid", PaymentMethod = "Mastercard", Reference = "ML-3001" },
                        new() { DateText = DateTime.Now.AddDays(-7).ToString("dd MMM yyyy"), Title = "Student Services Fee", Amount = 300.00m, Status = "Paid", PaymentMethod = "Visa", Reference = "ML-3002" },
                        new() { DateText = DateTime.Now.AddDays(-3).ToString("dd MMM yyyy"), Title = "Library Fine", Amount = 20.00m, Status = "Paid", PaymentMethod = "Bank Transfer", Reference = "ML-3003" }
                    ]
                }
            };
        }

        // Defensive check: every login account must have a ledger.
        foreach (var account in Store.LoginAccounts)
        {
            EnsureLedgerExists(account.StudentId);
        }

        // Defensive check: if old saved data had no cards, add default cards by account.
        foreach (var account in Store.LoginAccounts)
        {
            if (account.PaymentCards.Count == 0)
            {
                account.PaymentCards = account.StudentId switch
                {
                    "S1234567" =>
                    [
                        new() { CardholderName = account.FullName, Brand = "Visa", Last4Digits = "4242", Expiry = "09/28" },
                        new() { CardholderName = account.FullName, Brand = "Mastercard", Last4Digits = "1088", Expiry = "01/29" }
                    ],
                    "S2345678" =>
                    [
                        new() { CardholderName = account.FullName, Brand = "Visa", Last4Digits = "6731", Expiry = "12/27" },
                        new() { CardholderName = account.FullName, Brand = "Amex", Last4Digits = "3005", Expiry = "06/29" }
                    ],
                    "S3456789" =>
                    [
                        new() { CardholderName = account.FullName, Brand = "Mastercard", Last4Digits = "8844", Expiry = "03/30" },
                        new() { CardholderName = account.FullName, Brand = "Visa", Last4Digits = "5512", Expiry = "11/28" }
                    ],
                    _ => [ new() { CardholderName = account.FullName, Brand = "Visa", Last4Digits = "1111", Expiry = "01/30" } ]
                };
            }
        }

        // Old versions used Store.Transactions as one shared history.
        // It is cleared so all history now comes from AccountLedgers[studentId].Transactions.
        Store.Transactions = [];
    }

    private static DateTime ParseDate(string value)
    {
        return DateTime.TryParse(value, out var date) ? date : DateTime.MinValue;
    }
}
