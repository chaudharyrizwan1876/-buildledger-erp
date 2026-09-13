using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using Microsoft.Win32;

namespace BuildLedger.App.Views;

public partial class BackupView : UserControl
{
    public BackupView() => InitializeComponent();

    private BackupViewModel ViewModel => (BackupViewModel)DataContext;

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
        => await ViewModel.CreateBackupCommand.ExecuteAsync(null);

    private async void CreateBackupTo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Select backup destination folder" };
        if (dialog.ShowDialog() == true)
            await ViewModel.CreateBackupCommand.ExecuteAsync(dialog.FolderName);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Select backup file to restore", Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*" };
        if (dialog.ShowDialog() != true) return;

        var confirm = MessageBox.Show(
            "Restoring will replace the current database with this backup. A safety backup of the current data will be made first. Continue?",
            "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        await ViewModel.RestoreBackupCommand.ExecuteAsync(dialog.FileName);
    }
}
