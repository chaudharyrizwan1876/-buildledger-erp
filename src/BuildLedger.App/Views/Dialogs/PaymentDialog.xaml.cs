using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class PaymentDialogModel : ObservableObject
{
    [ObservableProperty] private string customerSearchText = string.Empty;
    [ObservableProperty] private bool showSuggestions;
    [ObservableProperty] private int? customerId;
    [ObservableProperty] private string? selectedCustomerLabel;
    [ObservableProperty] private string receiptNumber = string.Empty;
    [ObservableProperty] private DateTime? paymentDate = DateTime.Now;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private PaymentMethod method = PaymentMethod.Cash;
    [ObservableProperty] private PaymentPurpose purpose = PaymentPurpose.AgainstBalance;
    [ObservableProperty] private SaleSummaryDto? selectedSale;
    [ObservableProperty] private string? referenceNumber;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private string? errorMessage;

    public bool IsAgainstInvoice => Purpose == PaymentPurpose.AgainstInvoice;
    partial void OnPurposeChanged(PaymentPurpose value) => OnPropertyChanged(nameof(IsAgainstInvoice));

    public ObservableCollection<CustomerSearchResultDto> CustomerSuggestions { get; } = new();
    public ObservableCollection<SaleSummaryDto> CustomerSales { get; } = new();
    public IReadOnlyList<PaymentMethod> Methods { get; } = Enum.GetValues<PaymentMethod>();
    public IReadOnlyList<PaymentPurpose> Purposes { get; } = Enum.GetValues<PaymentPurpose>();

    public RecordPaymentDto ToDto()
    {
        var dto = new RecordPaymentDto
        {
            CustomerId = CustomerId ?? 0,
            ReceiptNumber = ReceiptNumber,
            PaymentDate = PaymentDate ?? DateTime.Now,
            Amount = Amount,
            Method = Method,
            Purpose = Purpose,
            ReferenceNumber = ReferenceNumber,
            Notes = Notes
        };
        if (Purpose == PaymentPurpose.AgainstInvoice && SelectedSale is not null)
            dto.Allocations.Add((SelectedSale.Id, Amount));
        return dto;
    }
}

public partial class PaymentDialog : Window
{
    public PaymentDialogModel Model { get; }
    private readonly Func<string, Task<IReadOnlyList<CustomerSearchResultDto>>> _searchCustomers;
    private readonly Func<int, Task<IReadOnlyList<SaleSummaryDto>>> _getCustomerSales;

    public PaymentDialog(string nextReceiptNumber,
        Func<string, Task<IReadOnlyList<CustomerSearchResultDto>>> searchCustomers,
        Func<int, Task<IReadOnlyList<SaleSummaryDto>>> getCustomerSales)
    {
        InitializeComponent();
        _searchCustomers = searchCustomers;
        _getCustomerSales = getCustomerSales;
        Model = new PaymentDialogModel { ReceiptNumber = nextReceiptNumber };
        DataContext = Model;
    }

    private async void CustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Model.CustomerSearchText) || Model.CustomerSearchText.Length < 2)
        {
            Model.ShowSuggestions = false;
            return;
        }
        var results = await _searchCustomers(Model.CustomerSearchText);
        Model.CustomerSuggestions.Clear();
        foreach (var r in results) Model.CustomerSuggestions.Add(r);
        Model.ShowSuggestions = Model.CustomerSuggestions.Count > 0;
    }

    private async void CustomerSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerSuggestions.SelectedItem is not CustomerSearchResultDto customer) return;
        Model.CustomerId = customer.Id;
        Model.SelectedCustomerLabel = $"Selected: {customer.FullName} — Balance: Rs. {customer.CurrentBalance:N0}";
        Model.ShowSuggestions = false;
        Model.CustomerSearchText = customer.FullName;

        Model.CustomerSales.Clear();
        var sales = await _getCustomerSales(customer.Id);
        foreach (var s in sales.Where(s => s.AmountDue > 0)) Model.CustomerSales.Add(s);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (Model.CustomerId is null) { Model.ErrorMessage = "Please select a customer."; return; }
        if (Model.Amount <= 0) { Model.ErrorMessage = "Amount must be greater than zero."; return; }
        if (Model.Purpose == PaymentPurpose.AgainstInvoice && Model.SelectedSale is null)
        { Model.ErrorMessage = "Please select an invoice to allocate this payment to."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
