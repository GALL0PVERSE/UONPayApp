namespace UONPayApp.Models;

/// <summary>
/// Display-only saved card model for the demo.
/// The app stores only brand, last 4 digits, and expiry, not a full card number.
/// </summary>
public class PaymentCard
{
    public string CardholderName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Last4Digits { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;

    // Text used in the payment method picker.
    public string DisplayName => $"{Brand} ending in {Last4Digits}";

    // Text used under the card item in the saved cards list.
    public string Subtitle => $"{CardholderName} • Exp {Expiry}";
}
