using System.Windows;
using BuildLedger.App.ViewModels;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class UserDialogModel : ObservableObject
{
    [ObservableProperty] private string title = "New User";
    [ObservableProperty] private int? id;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private int roleId;
    [ObservableProperty] private string? errorMessage;

    public string PasswordLabel => Id is null ? "Password *" : "New Password (leave blank to keep current)";
    public IReadOnlyList<RoleOption> Roles { get; set; } = Array.Empty<RoleOption>();

    public UserUpsertDto ToDto(string password) => new() { Id = Id, FullName = FullName, Username = Username, RoleId = RoleId, Password = password };
}

public partial class UserDialog : Window
{
    public UserDialogModel Model { get; }

    public UserDialog(IReadOnlyList<RoleOption> roles)
    {
        InitializeComponent();
        Model = new UserDialogModel { Roles = roles };
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.FullName)) { Model.ErrorMessage = "Full name is required."; return; }
        if (string.IsNullOrWhiteSpace(Model.Username)) { Model.ErrorMessage = "Username is required."; return; }
        if (Model.RoleId == 0) { Model.ErrorMessage = "Please select a role."; return; }
        if (Model.Id is null && PasswordInput.Password.Length < 6) { Model.ErrorMessage = "Password must be at least 6 characters."; return; }
        DialogResult = true;
    }

    public string Password => PasswordInput.Password;

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
