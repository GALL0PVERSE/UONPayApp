using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly AppDataService _dataService;

    public LoginPage()
    {
        InitializeComponent();

        // AppDataService works like the local backend for login validation.
        _dataService = App.Services.GetRequiredService<AppDataService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var studentIdOrEmail = StudentIdOrEmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Missing details", "Please enter your student ID or email and password.", "OK");
            return;
        }

        // TryLogin checks only the approved accounts seeded/saved in local JSON storage.
        // On success, it also sets Store.CurrentUser, which controls whose balance/cards/history appear.
        if (!_dataService.TryLogin(studentIdOrEmail, password))
        {
            await DisplayAlert("Login failed", "Only approved accounts saved in the backend data can log in. Check the student ID/email and password.", "OK");
            return;
        }

        Application.Current!.Windows[0].Page = new AppShell();
    }
}
