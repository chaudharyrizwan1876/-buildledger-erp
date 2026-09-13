using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class DashboardViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IReportService _reportService;
    private readonly INavigationService _navigation;

    public DashboardViewModel(IReportService reportService, INavigationService navigation)
    {
        _reportService = reportService;
        _navigation = navigation;
    }

    public DashboardSummaryDto? Summary { get; private set; }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Summary = await _reportService.GetDashboardAsync();
            OnPropertyChanged(nameof(Summary));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand] private void GoToNewSale() => _navigation.NavigateTo<SalesViewModel>();
    [RelayCommand] private void GoToNewCustomer() => _navigation.NavigateTo<CustomersViewModel>();
    [RelayCommand] private void GoToNewPayment() => _navigation.NavigateTo<PaymentsViewModel>();
    [RelayCommand] private void GoToNewPurchase() => _navigation.NavigateTo<PurchasesViewModel>();

    [RelayCommand] private async Task RefreshAsync() => await InitializeAsync();
}
