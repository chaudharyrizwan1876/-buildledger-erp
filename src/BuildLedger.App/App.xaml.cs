using System.IO;
using System.Windows;
using BuildLedger.App.Services;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views;
using BuildLedger.Infrastructure;
using BuildLedger.Infrastructure.Persistence;
using BuildLedger.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildLedger.App;

public partial class App : System.Windows.Application
{
    public static IHost AppHost { get; private set; } = null!;

    public static readonly string DataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BuildLedgerERP");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Directory.CreateDirectory(DataFolder);
        var dbPath = Path.Combine(DataFolder, "buildledger.db");
        var backupFolder = Path.Combine(DataFolder, "Backups");

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddBuildLedgerInfrastructure(dbPath, backupFolder);

                services.AddSingleton<INavigationService, NavigationService>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<SuppliersViewModel>();
                services.AddTransient<PurchasesViewModel>();
                services.AddTransient<SalesViewModel>();
                services.AddTransient<PaymentsViewModel>();
                services.AddTransient<InventoryViewModel>();
                services.AddTransient<ReportsViewModel>();
                services.AddTransient<ExpensesViewModel>();
                services.AddTransient<BackupViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<UsersViewModel>();
                services.AddTransient<AboutViewModel>();
            })
            .Build();

        await AppHost.StartAsync();

        using (var scope = AppHost.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BuildLedgerDbContext>();
            try
            {
                await DbInitializer.InitializeAsync(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize the database:\n{ex.Message}", "BuildLedger ERP",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
                return;
            }
        }

        ShowLogin();
    }

    public void ShowLogin()
    {
        var login = AppHost.Services.GetRequiredService<LoginViewModel>();
        var window = new LoginWindow { DataContext = login };
        login.LoginSucceeded += () =>
        {
            var main = new MainWindow { DataContext = AppHost.Services.GetRequiredService<MainViewModel>() };
            System.Windows.Application.Current.MainWindow = main;
            main.Show();
            window.Close();
        };
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
