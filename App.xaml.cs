using Microsoft.Extensions.DependencyInjection;
using UONPayApp.Services;
using UONPayApp.Views;

namespace UONPayApp;

public partial class App : Application
{
    public static IServiceProvider Services => Current!.Handler!.MauiContext!.Services;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var dataService = activationState?.Context?.Services.GetService<AppDataService>()
            ?? Services.GetRequiredService<AppDataService>();

        Page startPage = dataService.Store.IsLoggedIn
            ? new AppShell()
            : new NavigationPage(new LoginPage());

        return new Window(startPage);
    }
}
