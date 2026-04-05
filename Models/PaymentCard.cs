namespace UONPayApp.Models;

public class PaymentCard
{
    public string CardholderName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Last4Digits { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;

    public string DisplayName => $"{Brand} ending in {Last4Digits}";
    public string Subtitle => $"{CardholderName} • Exp {Expiry}";
}
