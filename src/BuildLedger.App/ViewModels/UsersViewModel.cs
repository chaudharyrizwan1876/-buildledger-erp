using BuildLedger.App.Services;
using System.Collections.ObjectModel;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public record RoleOption(int Id, string Name);

public partial class UsersViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IUserService _userService;
    public UsersViewModel(IUserService userService) => _userService = userService;

    public ObservableCollection<UserDto> Users { get; } = new();
    public ObservableCollection<RoleOption> Roles { get; } = new();

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        Roles.Clear();
        foreach (var r in await _userService.GetRolesAsync()) Roles.Add(new RoleOption(r.Id, r.Name));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Users.Clear();
        foreach (var u in await _userService.GetAllAsync()) Users.Add(u);
    }

    [RelayCommand]
    private async Task SaveUserAsync(UserUpsertDto dto)
    {
        ClearMessages();
        var result = await _userService.UpsertAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "User saved.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(UserDto user)
    {
        await _userService.SetActiveAsync(user.Id, !user.IsActive);
        await RefreshAsync();
    }
}
