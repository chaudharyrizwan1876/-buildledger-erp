using BuildLedger.App.Services;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class NavItem : ObservableObject
{
    public string Key { get; }
    public string Label { get; }
    public string Icon { get; }
    private readonly Action _navigate;

    [ObservableProperty] private bool isActive;

    public NavItem(string key, string label, string icon, Action navigate)
    {
        Key = key; Label = label; Icon = icon; _navigate = navigate;
    }

    [RelayCommand]
    private void Select() => _navigate();
}

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ICurrentUserContext _currentUser;

    public string UserFullName => _currentUser.FullName ?? string.Empty;
    public string UserRole => _currentUser.Role?.ToString() ?? string.Empty;

    public List<NavItem> NavItems { get; }

    [ObservableProperty]
    private object? currentView;

    public event Action? SignedOut;

    public MainViewModel(INavigationService navigation, ICurrentUserContext currentUser)
    {
        _navigation = navigation;
        _currentUser = currentUser;

        NavItems = new List<NavItem>
        {
            new("Dashboard", "Dashboard", "", () => Navigate<DashboardViewModel>()),
            new("Customers", "Customers", "", () => Navigate<CustomersViewModel>()),
            new("Products", "Products & Units", "", () => Navigate<ProductsViewModel>()),
            new("Suppliers", "Suppliers", "", () => Navigate<SuppliersViewModel>()),
            new("Purchases", "Purchases", "", () => Navigate<PurchasesViewModel>()),
            new("Sales", "Sales & Delivery", "", () => Navigate<SalesViewModel>()),
            new("Payments", "Payments & Receipts", "", () => Navigate<PaymentsViewModel>()),
            new("Inventory", "Inventory", "", () => Navigate<InventoryViewModel>()),
            new("Expenses", "Expenses", "", () => Navigate<ExpensesViewModel>()),
            new("Reports", "Reports Center", "", () => Navigate<ReportsViewModel>()),
            new("Backup", "Backup & Restore", "", () => Navigate<BackupViewModel>()),
            new("Users", "User Management", "", () => Navigate<UsersViewModel>()),
            new("Settings", "Settings", "", () => Navigate<SettingsViewModel>()),
            new("About", "About", "", () => Navigate<AboutViewModel>()),
        };

        _navigation.CurrentViewModelChanged += () =>
        {
            CurrentView = _navigation.CurrentViewModel;
            foreach (var item in NavItems) item.IsActive = item.Key == _navigation.CurrentKey;
        };

        Navigate<DashboardViewModel>();
    }

    private void Navigate<T>() where T : ViewModelBase => _navigation.NavigateTo<T>();

    [RelayCommand]
    private void SignOut()
    {
        _currentUser.SignOut();
        SignedOut?.Invoke();
    }
}
