using System.Text.Json;
using UONPayApp.Models;

namespace UONPayApp.Services;

public class AppDataService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AppDataStore Store { get; private set; } = new();

    public AppDataService()
    {
        _filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "uonpayapp-data.json");
        Load();
        SeedDefaults();
        Save();
    }

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
            Store = new AppDataStore();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Store, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

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

        EnsureLedgerExists(matchedAccount.StudentId);
        Store.IsLoggedIn = true;
        Store.CurrentUser = UserProfile.FromLoginAccount(matchedAccount);
        Save();
        return true;
    }

    public void Logout()
    {
        Store.IsLoggedIn = false;
        Store.CurrentUser = null;
        Save();
    }

    public decimal GetOutstandingBalance()
    {
        var ledger = GetCurrentLedger();
        var totalPaid = ledger.Transactions.Sum(x => x.Amount);
        var balance = ledger.TotalDue - totalPaid;
        return balance < 0 ? 0 : balance;
    }

    public List<TransactionItem> GetCurrentTransactions()
    {
        return GetCurrentLedger()
            .Transactions
            .OrderByDescending(x => ParseDate(x.DateText))
            .ToList();
    }

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

    private void EnsureLedgerExists(string studentId)
    {
        if (Store.AccountLedgers.ContainsKey(studentId))
        {
            return;
        }

        Store.AccountLedgers[studentId] = new AccountLedger();
    }

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

        foreach (var account in Store.LoginAccounts)
        {
            EnsureLedgerExists(account.StudentId);
        }

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

        Store.Transactions = [];
    }

    private static DateTime ParseDate(string value)
    {
        return DateTime.TryParse(value, out var date) ? date : DateTime.MinValue;
    }
}
