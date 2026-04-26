using Microsoft.Maui.Devices;
using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly AppDataService _dataService;

    public PaymentsPage()
    {
        InitializeComponent();

        // Get the shared local data service registered in MauiProgram.cs.
        // This service handles login accounts, separate ledgers, saved cards, and JSON storage.
        _dataService = App.Services.GetRequiredService<AppDataService>();

        // Payment categories shown in the payment type dropdown.
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

        // Reload local JSON storage each time the page appears so the screen is up to date.
        _dataService.Load();

        var user = _dataService.Store.CurrentUser;

        // Cards are fetched for the logged-in student only.
        // Example: S1234567 and S2345678 will see different cards.
        var cards = _dataService.GetCurrentPaymentCards();

        AccountPaymentInfoLabel.Text = $"Payment methods for {user?.FullName ?? "this account"}";
        SavedCardsView.ItemsSource = cards;
        PaymentMethodPicker.ItemsSource = cards.Select(x => x.DisplayName).ToList();

        if (PaymentMethodPicker.ItemsSource is IList<string> methods && methods.Count > 0)
        {
            PaymentMethodPicker.SelectedIndex = 0;
        }

        // Outstanding balance is calculated from the current student's own ledger.
        BalanceLabel.Text = _dataService.GetOutstandingBalance().ToString("C");
    }

    private async void OnPayNowClicked(object sender, EventArgs e)
    {
        var feeType = PaymentTypePicker.SelectedItem?.ToString() ?? string.Empty;
        var reference = ReferenceEntry.Text?.Trim() ?? string.Empty;
        var paymentMethod = PaymentMethodPicker.SelectedItem?.ToString() ?? string.Empty;

        // Validate the payment amount before saving anything.
        if (!decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Invalid amount", "Please enter a valid payment amount.", "OK");
            return;
        }

        // A payment needs both a fee type and a selected card/payment method.
        if (string.IsNullOrWhiteSpace(feeType) || string.IsNullOrWhiteSpace(paymentMethod))
        {
            await DisplayAlert("Missing details", "Please select the fee type and payment method.", "OK");
            return;
        }

        // Auto-generate a transaction reference if the user leaves it empty.
        if (string.IsNullOrWhiteSpace(reference))
        {
            reference = $"TXN-{DateTime.Now:yyyyMMddHHmmss}";
        }

        // Save the payment into the logged-in student's ledger only.
        // This updates that student's bill history and reduces only that student's balance.
        _dataService.AddTransaction(feeType, amount, paymentMethod, reference);

        // Refresh the page values after saving the payment.
        BalanceLabel.Text = _dataService.GetOutstandingBalance().ToString("C");
        AmountEntry.Text = string.Empty;
        ReferenceEntry.Text = string.Empty;

        // VIBRATION EFFECT:
        // This runs immediately before the success popup appears, so the phone vibrates
        // at the same moment the payment complete message is shown.
        // Android permission is in Platforms/Android/AndroidManifest.xml:
        // <uses-permission android:name="android.permission.VIBRATE" />
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
        }
        catch
        {
            // Some devices/emulators may not support vibration or may have vibration disabled.
            // Ignore the error so the payment success popup still appears.
        }

        await DisplayAlert("Payment complete", $"Your payment for {feeType} was saved using {paymentMethod}.", "OK");
    }
}
