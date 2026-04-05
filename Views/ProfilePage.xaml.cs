using UONPayApp.Services;

namespace UONPayApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly AppDataService _dataService;

    public ProfilePage()
    {
        InitializeComponent();
        _dataService = App.Services.GetRequiredService<AppDataService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dataService.Load();

        var user = _dataService.Store.CurrentUser;
        FullNameLabel.Text = user?.FullName ?? "Student";
        StudentIdLabel.Text = $"Student ID: {user?.StudentId ?? "-"}";
        EmailLabel.Text = $"Email: {user?.Email ?? "-"}";
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        _dataService.Logout();
        Application.Current!.Windows[0].Page = new NavigationPage(new LoginPage());
    }
}
