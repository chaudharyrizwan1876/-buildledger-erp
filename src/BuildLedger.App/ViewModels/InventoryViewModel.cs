using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class InventoryViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly ICurrentUserContext _currentUser;

    public InventoryViewModel(IInventoryService inventoryService, IProductService productService, ICurrentUserContext currentUser)
    {
        _inventoryService = inventoryService;
        _productService = productService;
        _currentUser = currentUser;
    }

    public ObservableCollection<StockMovementDto> Movements { get; } = new();
    public ObservableCollection<ProductDto> Products { get; } = new();
    public decimal StockValuation { get; private set; }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        Products.Clear();
        foreach (var p in await _productService.GetAllAsync()) Products.Add(p);
        StockValuation = await _inventoryService.GetStockValuationAsync();
        OnPropertyChanged(nameof(StockValuation));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Movements.Clear();
        foreach (var m in await _inventoryService.GetMovementsAsync(null, null)) Movements.Add(m);
    }

    [RelayCommand]
    private async Task AdjustStockAsync(StockAdjustmentDto dto)
    {
        ClearMessages();
        var result = await _inventoryService.AdjustStockAsync(dto, _currentUser.UserId);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Stock adjusted.";
        await InitializeAsync();
    }
}
