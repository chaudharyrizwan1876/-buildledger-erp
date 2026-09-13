using BuildLedger.App.Services;
using System.Collections.ObjectModel;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class PaymentsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;
    private readonly ISalesService _salesService;

    public PaymentsViewModel(IPaymentService paymentService, ICustomerService customerService, ISalesService salesService)
    {
        _paymentService = paymentService;
        _customerService = customerService;
        _salesService = salesService;
    }

    public ObservableCollection<PaymentSummaryDto> Payments { get; } = new();

    public async Task InitializeAsync() => await RefreshAsync();

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Payments.Clear();
        foreach (var p in await _paymentService.SearchAsync(null, null)) Payments.Add(p);
    }

    public Task<IReadOnlyList<CustomerSearchResultDto>> SearchCustomersAsync(string query) => _customerService.SearchAsync(query);
    public Task<IReadOnlyList<SaleSummaryDto>> GetCustomerSalesAsync(int customerId) => _salesService.SearchAsync(null, customerId, null);
    public Task<string> GenerateNextReceiptNumberAsync() => _paymentService.GenerateNextReceiptNumberAsync();

    [RelayCommand]
    private async Task RecordPaymentAsync(RecordPaymentDto dto)
    {
        ClearMessages();
        var result = await _paymentService.RecordPaymentAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Payment recorded.";
        await RefreshAsync();
    }
}
