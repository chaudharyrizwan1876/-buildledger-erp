using BuildLedger.App.Services;
using System.Collections.ObjectModel;
using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class ReportsViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IReportService _reportService;
    public ReportsViewModel(IReportService reportService) => _reportService = reportService;

    public ObservableCollection<SaleSummaryDto> SalesReport { get; } = new();
    public ObservableCollection<PaymentSummaryDto> PaymentsReport { get; } = new();
    public ObservableCollection<CustomerSearchResultDto> Receivables { get; } = new();
    public ObservableCollection<SupplierDto> SupplierBalances { get; } = new();

    [ObservableProperty] private DateTime fromDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime toDate = DateTime.Today;

    public async Task InitializeAsync() => await RunAllAsync();

    [RelayCommand]
    private async Task RunAllAsync()
    {
        var range = new DateRange(FromDate, ToDate.AddDays(1).AddTicks(-1));

        SalesReport.Clear();
        foreach (var s in await _reportService.SalesReportAsync(range, null, null)) SalesReport.Add(s);

        PaymentsReport.Clear();
        foreach (var p in await _reportService.PaymentsReportAsync(range, null)) PaymentsReport.Add(p);

        Receivables.Clear();
        foreach (var r in await _reportService.ReceivablesReportAsync()) Receivables.Add(r);

        SupplierBalances.Clear();
        foreach (var s in await _reportService.SupplierBalancesReportAsync()) SupplierBalances.Add(s);
    }

    [RelayCommand] private async Task SetToday() { FromDate = DateRange.Today().From; ToDate = DateTime.Today; await RunAllAsync(); }
    [RelayCommand] private async Task SetThisWeek() { var r = DateRange.ThisWeek(); FromDate = r.From; ToDate = DateTime.Today; await RunAllAsync(); }
    [RelayCommand] private async Task SetThisMonth() { var r = DateRange.ThisMonth(); FromDate = r.From; ToDate = DateTime.Today; await RunAllAsync(); }
    [RelayCommand] private async Task SetLastMonth() { var r = DateRange.LastMonth(); FromDate = r.From; ToDate = r.To; await RunAllAsync(); }
}
