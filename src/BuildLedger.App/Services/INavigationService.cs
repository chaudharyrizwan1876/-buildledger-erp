using BuildLedger.App.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLedger.App.Services;

public interface INavigationService
{
    ObservableObject? CurrentViewModel { get; }
    event Action? CurrentViewModelChanged;
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    string CurrentKey { get; }
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    public NavigationService(IServiceProvider provider) => _provider = provider;

    public ObservableObject? CurrentViewModel { get; private set; }
    public string CurrentKey { get; private set; } = string.Empty;
    public event Action? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        CurrentViewModel = _provider.GetRequiredService(typeof(TViewModel)) as ObservableObject;
        CurrentKey = typeof(TViewModel).Name;
        CurrentViewModelChanged?.Invoke();

        if (CurrentViewModel is IAsyncInitializable initializable)
        {
            _ = initializable.InitializeAsync();
        }
    }
}

public interface IAsyncInitializable
{
    Task InitializeAsync();
}
