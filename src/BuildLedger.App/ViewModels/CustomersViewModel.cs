using System.Collections.ObjectModel;
using BuildLedger.App.Services;
using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class CustomersViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ICustomerService _customerService;

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public ObservableCollection<CustomerSearchResultDto> Customers { get; } = new();
    public ObservableCollection<LedgerRowDto> Ledger { get; } = new();
    public ObservableCollection<SiteDto> Sites { get; } = new();

    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private CustomerSearchResultDto? selectedCustomer;
    [ObservableProperty] private CustomerDetailDto? selectedCustomerDetail;
    [ObservableProperty] private SiteDto? selectedSite;

    public async Task InitializeAsync() => await SearchAsync();

    partial void OnSearchQueryChanged(string value) => _ = SearchAsync();

    [RelayCommand]
    private async Task SearchAsync()
    {
        var results = await _customerService.SearchAsync(SearchQuery);
        Customers.Clear();
        foreach (var c in results) Customers.Add(c);
    }

    partial void OnSelectedCustomerChanged(CustomerSearchResultDto? value)
    {
        _ = LoadDetailAsync(value?.Id);
    }

    private async Task LoadDetailAsync(int? id)
    {
        Ledger.Clear();
        Sites.Clear();
        SelectedCustomerDetail = null;
        if (id is null) return;

        SelectedCustomerDetail = await _customerService.GetByIdAsync(id.Value);
        if (SelectedCustomerDetail is null) return;

        foreach (var s in SelectedCustomerDetail.Sites) Sites.Add(s);

        var ledgerRows = await _customerService.GetLedgerAsync(id.Value, null, null);
        foreach (var row in ledgerRows) Ledger.Add(row);
    }

    [RelayCommand]
    private async Task SaveCustomerAsync(CustomerUpsertDto dto)
    {
        ClearMessages();
        var result = await _customerService.UpsertAsync(dto);
        if (!result.Success)
        {
            ErrorMessage = result.Error;
            return;
        }
        SuccessMessage = "Customer saved.";
        await SearchAsync();
    }

    [RelayCommand]
    private async Task SaveSiteAsync(SiteUpsertDto dto)
    {
        ClearMessages();
        var result = await _customerService.UpsertSiteAsync(dto);
        if (!result.Success)
        {
            ErrorMessage = result.Error;
            return;
        }
        SuccessMessage = "Site saved.";
        await LoadDetailAsync(dto.CustomerId);
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(CustomerSearchResultDto customer)
    {
        await _customerService.SetActiveAsync(customer.Id, !customer.IsActive);
        await SearchAsync();
    }
}
