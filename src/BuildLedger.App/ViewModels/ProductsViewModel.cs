using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class ProductsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IProductService _productService;
    public ProductsViewModel(IProductService productService) => _productService = productService;

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<UnitDto> Units { get; } = new();
    public ObservableCollection<ProductCategoryDto> Categories { get; } = new();

    [ObservableProperty] private bool showInactive;

    public async Task InitializeAsync() => await RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var products = await _productService.GetAllAsync(ShowInactive);
        Products.Clear();
        foreach (var p in products) Products.Add(p);

        Units.Clear();
        foreach (var u in await _productService.GetUnitsAsync()) Units.Add(u);

        Categories.Clear();
        foreach (var c in await _productService.GetCategoriesAsync()) Categories.Add(c);
    }

    [RelayCommand]
    private async Task SaveProductAsync(ProductUpsertDto dto)
    {
        ClearMessages();
        var result = await _productService.UpsertProductAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Product saved.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(ProductDto product)
    {
        await _productService.SetProductActiveAsync(product.Id, !product.IsActive);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SaveUnitAsync(UnitUpsertDto dto)
    {
        await _productService.UpsertUnitAsync(dto);
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SaveCategoryAsync(ProductCategoryUpsertDto dto)
    {
        await _productService.UpsertCategoryAsync(dto);
        await RefreshAsync();
    }
}
