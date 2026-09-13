using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class SalesViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ISalesService _salesService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly ISettingsService _settingsService;

    public SalesViewModel(ISalesService salesService, ICustomerService customerService, IProductService productService, ISettingsService settingsService)
    {
        _salesService = salesService;
        _customerService = customerService;
        _productService = productService;
        _settingsService = settingsService;
    }

    public ObservableCollection<SaleSummaryDto> Sales { get; } = new();
    public ObservableCollection<ProductDto> Products { get; } = new();

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        Products.Clear();
        foreach (var p in await _productService.GetAllAsync()) Products.Add(p);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Sales.Clear();
        foreach (var s in await _salesService.SearchAsync(null, null, null)) Sales.Add(s);
    }

    public Task<IReadOnlyList<CustomerSearchResultDto>> SearchCustomersAsync(string query) => _customerService.SearchAsync(query);
    public Task<IReadOnlyList<SiteDto>> GetSitesAsync(int customerId) => _customerService.GetSitesForCustomerAsync(customerId);
    public Task<string> GenerateNextInvoiceNumberAsync() => _salesService.GenerateNextInvoiceNumberAsync();
    public Task<SaleDetailDto?> GetSaleDetailAsync(int saleId) => _salesService.GetByIdAsync(saleId);
    public async Task<string> GetBusinessNameAsync() => await _settingsService.GetAsync("BusinessName") ?? "BuildLedger ERP";

    [RelayCommand]
    private async Task CreateSaleAsync(CreateSaleDto dto)
    {
        ClearMessages();
        var result = await _salesService.CreateSaleAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Sale recorded.";
        await RefreshAsync();
    }
}
