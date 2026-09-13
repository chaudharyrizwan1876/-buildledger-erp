using BuildLedger.App.Services;
using System.Collections.ObjectModel;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class BackupViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IBackupService _backupService;
    public BackupViewModel(IBackupService backupService) => _backupService = backupService;

    public ObservableCollection<BackupHistoryDto> History { get; } = new();

    public async Task InitializeAsync() => await RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        History.Clear();
        foreach (var h in await _backupService.GetHistoryAsync()) History.Add(h);
    }

    [RelayCommand]
    private async Task CreateBackupAsync(string? destinationFolder)
    {
        ClearMessages();
        IsBusy = true;
        try
        {
            var result = await _backupService.CreateBackupAsync(destinationFolder);
            if (!result.Success) { ErrorMessage = result.Error; return; }
            SuccessMessage = $"Backup created: {result.Data}";
            await RefreshAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync(string filePath)
    {
        ClearMessages();
        IsBusy = true;
        try
        {
            var result = await _backupService.RestoreBackupAsync(filePath);
            if (!result.Success) { ErrorMessage = result.Error; return; }
            SuccessMessage = "Restore complete. Please restart the application.";
        }
        finally { IsBusy = false; }
    }
}
