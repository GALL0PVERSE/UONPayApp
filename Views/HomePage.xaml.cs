using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class HomePage : ContentPage
{
    private readonly AppDataService _dataService;

    public HomePage()
    {
        InitializeComponent();
        _dataService = App.Services.GetRequiredService<AppDataService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dataService.Load();

        var user = _dataService.Store.CurrentUser;
        WelcomeNameLabel.Text = $"Welcome, {user?.FullName ?? "Student"}";
        StudentIdLabel.Text = $"Student ID: {user?.StudentId ?? "-"}";
        EmailLabel.Text = $"Email: {user?.Email ?? "-"}";
        OutstandingBalanceLabel.Text = _dataService.GetOutstandingBalance().ToString("C");
        RecentTransactionsView.ItemsSource = _dataService.GetCurrentTransactions().Take(5).ToList();
    }
}
