using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public event Action? LoginSucceeded;

    [ObservableProperty]
    private string username = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync(object? passwordBoxParam)
    {
        ClearMessages();
        var password = passwordBoxParam as string ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter both username and password.";
            return;
        }

        IsBusy = true;
        try
        {
            // Keep the spinner visible for a perceptible moment even on a near-instant local login,
            // so the user gets clear feedback that their click registered.
            var loginTask = _authService.LoginAsync(Username.Trim(), password);
            await Task.WhenAll(loginTask, Task.Delay(400));
            var result = loginTask.Result;

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Login failed.";
                return;
            }
            LoginSucceeded?.Invoke();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
