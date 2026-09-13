using System.IO;
using System.Windows;
using System.Windows.Threading;
using BuildLedger.App.Services;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views;
using BuildLedger.Application.Interfaces;
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

    private DispatcherTimer? _autoBackupTimer;
    private DateTime _lastAutoBackupCheckDate = DateTime.MinValue;

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

        StartAutoBackupTimer();
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

    /// <summary>
    /// Checks once a minute whether it's time for the configured daily automatic backup.
    /// Uses a fresh DI scope per check so it never holds a long-lived DbContext.
    /// </summary>
    private void StartAutoBackupTimer()
    {
        _autoBackupTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _autoBackupTimer.Tick += async (_, _) => await CheckAutoBackupAsync();
        _autoBackupTimer.Start();
    }

    private async Task CheckAutoBackupAsync()
    {
        using var scope = AppHost.Services.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var backup = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var enabled = (await settings.GetAsync("AutoBackupEnabled")) == "true";
        if (!enabled) return;

        var configuredTime = await settings.GetAsync("AutoBackupTime") ?? "20:00";
        if (!TimeSpan.TryParse(configuredTime, out var targetTime)) return;

        var now = DateTime.Now;
        if (now.TimeOfDay < targetTime || now.TimeOfDay >= targetTime.Add(TimeSpan.FromMinutes(1))) return;
        if (_lastAutoBackupCheckDate.Date == now.Date) return;

        var history = await backup.GetHistoryAsync();
        var alreadyDoneToday = history.Any(h => !h.IsManual && h.BackupDateUtc.ToLocalTime().Date == now.Date);
        if (alreadyDoneToday)
        {
            _lastAutoBackupCheckDate = now.Date;
            return;
        }

        await backup.CreateBackupAsync(isManual: false);
        _lastAutoBackupCheckDate = now.Date;
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
