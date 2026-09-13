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
    public bool IsHeader { get; }
    private readonly Action? _navigate;

    [ObservableProperty] private bool isActive;

    public NavItem(string key, string label, string icon, Action navigate)
    {
        Key = key; Label = label; Icon = icon; _navigate = navigate;
    }

    /// <summary>Constructs a non-interactive section header row.</summary>
    public NavItem(string headerText)
    {
        Key = string.Empty; Label = headerText; Icon = string.Empty; IsHeader = true;
    }

    [RelayCommand]
    private void Select() => _navigate?.Invoke();
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
            new("OVERVIEW"),
            new(nameof(DashboardViewModel), "Dashboard", "", () => Navigate<DashboardViewModel>()),

            new("SALES & CUSTOMERS"),
            new(nameof(CustomersViewModel), "Customers", "", () => Navigate<CustomersViewModel>()),
            new(nameof(SalesViewModel), "Sales & Delivery", "", () => Navigate<SalesViewModel>()),
            new(nameof(PaymentsViewModel), "Payments & Receipts", "", () => Navigate<PaymentsViewModel>()),

            new("CATALOG & STOCK"),
            new(nameof(ProductsViewModel), "Products & Units", "", () => Navigate<ProductsViewModel>()),
            new(nameof(SuppliersViewModel), "Suppliers", "", () => Navigate<SuppliersViewModel>()),
            new(nameof(PurchasesViewModel), "Purchases", "", () => Navigate<PurchasesViewModel>()),
            new(nameof(InventoryViewModel), "Inventory", "", () => Navigate<InventoryViewModel>()),

            new("FINANCE"),
            new(nameof(ExpensesViewModel), "Expenses", "", () => Navigate<ExpensesViewModel>()),
            new(nameof(ReportsViewModel), "Reports Center", "", () => Navigate<ReportsViewModel>()),

            new("SYSTEM"),
            new(nameof(BackupViewModel), "Backup & Restore", "", () => Navigate<BackupViewModel>()),
            new(nameof(UsersViewModel), "User Management", "", () => Navigate<UsersViewModel>()),
            new(nameof(SettingsViewModel), "Settings", "", () => Navigate<SettingsViewModel>()),
            new(nameof(AboutViewModel), "About", "", () => Navigate<AboutViewModel>()),
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
