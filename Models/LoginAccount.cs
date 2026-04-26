namespace UONPayApp.Models;

/// <summary>
/// Approved student account used for local login.
/// PaymentCards is stored here so each student has different cards on the payment page.
/// </summary>
public class LoginAccount
{
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<PaymentCard> PaymentCards { get; set; } = [];
}
