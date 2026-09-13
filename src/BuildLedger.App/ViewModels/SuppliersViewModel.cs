using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class SuppliersViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ISupplierService _supplierService;
    public SuppliersViewModel(ISupplierService supplierService) => _supplierService = supplierService;

    public ObservableCollection<SupplierDto> Suppliers { get; } = new();

    public async Task InitializeAsync() => await RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Suppliers.Clear();
        foreach (var s in await _supplierService.GetAllAsync()) Suppliers.Add(s);
    }

    [RelayCommand]
    private async Task SaveSupplierAsync(SupplierUpsertDto dto)
    {
        ClearMessages();
        var result = await _supplierService.UpsertAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Supplier saved.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(SupplierDto supplier)
    {
        await _supplierService.SetActiveAsync(supplier.Id, !supplier.IsActive);
        await RefreshAsync();
    }
}
