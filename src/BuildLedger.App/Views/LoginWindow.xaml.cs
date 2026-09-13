using System.Windows;
using System.Windows.Input;
using BuildLedger.App.ViewModels;

namespace BuildLedger.App.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private async void SignIn_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoginCommand.ExecuteAsync(PasswordInput.Password);
    }

    private async void PasswordInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await ViewModel.LoginCommand.ExecuteAsync(PasswordInput.Password);
        }
    }
}
