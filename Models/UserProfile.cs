namespace UONPayApp.Models;

public class UserProfile
{
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public static UserProfile FromLoginAccount(LoginAccount account)
    {
        return new UserProfile
        {
            FullName = account.FullName,
            StudentId = account.StudentId,
            Email = account.Email
        };
    }
}
