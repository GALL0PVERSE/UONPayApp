using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class HistoryPage : ContentPage
{
    private readonly AppDataService _dataService;

    public HistoryPage()
    {
        InitializeComponent();
        _dataService = App.Services.GetRequiredService<AppDataService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dataService.Load();
        TransactionsView.ItemsSource = _dataService.GetCurrentTransactions();
    }
}
