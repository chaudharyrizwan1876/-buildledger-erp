using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class PurchasesViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;

    public PurchasesViewModel(IPurchaseService purchaseService, ISupplierService supplierService, IProductService productService)
    {
        _purchaseService = purchaseService;
        _supplierService = supplierService;
        _productService = productService;
    }

    public ObservableCollection<PurchaseSummaryDto> Purchases { get; } = new();
    public ObservableCollection<SupplierDto> Suppliers { get; } = new();
    public ObservableCollection<ProductDto> Products { get; } = new();

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        Suppliers.Clear();
        foreach (var s in await _supplierService.GetAllAsync()) Suppliers.Add(s);
        Products.Clear();
        foreach (var p in await _productService.GetAllAsync()) Products.Add(p);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Purchases.Clear();
        foreach (var p in await _purchaseService.GetAllAsync()) Purchases.Add(p);
    }

    public Task<string> GenerateNextInvoiceNumberAsync() => _purchaseService.GenerateNextInvoiceNumberAsync();

    [RelayCommand]
    private async Task CreatePurchaseAsync(CreatePurchaseDto dto)
    {
        ClearMessages();
        var result = await _purchaseService.CreatePurchaseAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Purchase recorded and stock updated.";
        await InitializeAsync();
    }
}
