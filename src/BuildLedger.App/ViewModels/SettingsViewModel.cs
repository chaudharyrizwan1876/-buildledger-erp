using BuildLedger.App.Services;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ISettingsService _settingsService;
    public SettingsViewModel(ISettingsService settingsService) => _settingsService = settingsService;

    [ObservableProperty] private string businessName = string.Empty;
    [ObservableProperty] private bool allowNegativeStock;
    [ObservableProperty] private bool isDarkTheme;
    [ObservableProperty] private bool autoBackupEnabled;
    [ObservableProperty] private string autoBackupTime = "20:00";

    public async Task InitializeAsync()
    {
        var all = await _settingsService.GetAllAsync();
        BusinessName = all.GetValueOrDefault("BusinessName", "");
        AllowNegativeStock = all.GetValueOrDefault("AllowNegativeStock", "false") == "true";
        AutoBackupEnabled = all.GetValueOrDefault("AutoBackupEnabled", "true") == "true";
        AutoBackupTime = all.GetValueOrDefault("AutoBackupTime", "20:00");
        IsDarkTheme = ThemeManager.Current == AppTheme.Dark;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ClearMessages();
        await _settingsService.SetAsync("BusinessName", BusinessName);
        await _settingsService.SetAsync("AllowNegativeStock", AllowNegativeStock ? "true" : "false");
        await _settingsService.SetAsync("AutoBackupEnabled", AutoBackupEnabled ? "true" : "false");
        await _settingsService.SetAsync("AutoBackupTime", AutoBackupTime);
        SuccessMessage = "Settings saved.";
    }

    partial void OnIsDarkThemeChanged(bool value) => ThemeManager.Apply(value ? AppTheme.Dark : AppTheme.Light);
}
