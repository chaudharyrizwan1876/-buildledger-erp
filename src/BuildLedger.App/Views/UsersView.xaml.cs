using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class UsersView : UserControl
{
    public UsersView() => InitializeComponent();

    private UsersViewModel ViewModel => (UsersViewModel)DataContext;

    private async void NewUser_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UserDialog(ViewModel.Roles.ToList()) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveUserCommand.ExecuteAsync(dialog.Model.ToDto(dialog.Password));
    }
}
