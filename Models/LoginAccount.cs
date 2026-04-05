namespace UONPayApp.Models;

public class LoginAccount
{
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<PaymentCard> PaymentCards { get; set; } = [];
}
