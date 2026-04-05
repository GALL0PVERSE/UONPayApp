using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly AppDataService _dataService;

    public LoginPage()
    {
        InitializeComponent();
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

        if (!_dataService.TryLogin(studentIdOrEmail, password))
        {
            await DisplayAlert("Login failed", "Only approved accounts saved in the backend data can log in. Check the student ID/email and password.", "OK");
            return;
        }

        Application.Current!.Windows[0].Page = new AppShell();
    }
}
