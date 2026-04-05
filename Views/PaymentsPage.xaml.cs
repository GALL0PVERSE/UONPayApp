using Microsoft.Maui.Devices;
using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly AppDataService _dataService;

    public PaymentsPage()
    {
        InitializeComponent();
        _dataService = App.Services.GetRequiredService<AppDataService>();

        PaymentTypePicker.ItemsSource = new List<string>
        {
            "Tuition Fee",
            "Accommodation",
            "Library Fine",
            "Other Fees"
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dataService.Load();

        var user = _dataService.Store.CurrentUser;
        var cards = _dataService.GetCurrentPaymentCards();

        AccountPaymentInfoLabel.Text = $"Payment methods for {user?.FullName ?? "this account"}";
        SavedCardsView.ItemsSource = cards;
        PaymentMethodPicker.ItemsSource = cards.Select(x => x.DisplayName).ToList();

        if (PaymentMethodPicker.ItemsSource is IList<string> methods && methods.Count > 0)
        {
            PaymentMethodPicker.SelectedIndex = 0;
        }

        BalanceLabel.Text = _dataService.GetOutstandingBalance().ToString("C");
    }

    private async void OnPayNowClicked(object sender, EventArgs e)
    {
        var feeType = PaymentTypePicker.SelectedItem?.ToString() ?? string.Empty;
        var reference = ReferenceEntry.Text?.Trim() ?? string.Empty;
        var paymentMethod = PaymentMethodPicker.SelectedItem?.ToString() ?? string.Empty;

        if (!decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Invalid amount", "Please enter a valid payment amount.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(feeType) || string.IsNullOrWhiteSpace(paymentMethod))
        {
            await DisplayAlert("Missing details", "Please select the fee type and payment method.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(reference))
        {
            reference = $"TXN-{DateTime.Now:yyyyMMddHHmmss}";
        }

        _dataService.AddTransaction(feeType, amount, paymentMethod, reference);
        BalanceLabel.Text = _dataService.GetOutstandingBalance().ToString("C");
        AmountEntry.Text = string.Empty;
        ReferenceEntry.Text = string.Empty;

        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
        }
        catch
        {
        }

        await DisplayAlert("Payment complete", $"Your payment for {feeType} was saved using {paymentMethod}.", "OK");
    }
}
