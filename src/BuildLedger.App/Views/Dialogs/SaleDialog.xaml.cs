using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class SaleLineRow : ObservableObject
{
    [ObservableProperty] private int productId;
    [ObservableProperty] private decimal availableStock;
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private decimal unitRate;
    public decimal LineTotal => Quantity * UnitRate;

    partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnUnitRateChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
}

public partial class SaleDialogModel : ObservableObject
{
    [ObservableProperty] private string customerSearchText = string.Empty;
    [ObservableProperty] private bool showSuggestions;
    [ObservableProperty] private int? customerId;
    [ObservableProperty] private string? selectedCustomerLabel;
    [ObservableProperty] private int? siteId;
    [ObservableProperty] private string invoiceNumber = string.Empty;
    [ObservableProperty] private DateTime? saleDate = DateTime.Now;
    [ObservableProperty] private decimal discountAmount;
    [ObservableProperty] private decimal? amountPaidNow;
    [ObservableProperty] private PaymentMethod paymentMethod = PaymentMethod.Cash;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private string? errorMessage;

    public ObservableCollection<CustomerSearchResultDto> CustomerSuggestions { get; } = new();
    public ObservableCollection<SiteDto> Sites { get; } = new();
    public ObservableCollection<SaleLineRow> Lines { get; } = new();
    public IReadOnlyList<ProductDto> Products { get; set; } = Array.Empty<ProductDto>();
    public IReadOnlyList<PaymentMethod> PaymentMethods { get; } = Enum.GetValues<PaymentMethod>();

    public decimal SubTotal => Lines.Sum(l => l.LineTotal);
    public decimal Total => SubTotal - DiscountAmount;

    partial void OnDiscountAmountChanged(decimal value) => OnPropertyChanged(nameof(Total));

    public void RecalculateTotal() { OnPropertyChanged(nameof(SubTotal)); OnPropertyChanged(nameof(Total)); }

    public CreateSaleDto ToDto() => new()
    {
        CustomerId = CustomerId ?? 0,
        SiteId = SiteId,
        InvoiceNumber = InvoiceNumber,
        SaleDate = SaleDate ?? DateTime.Now,
        DiscountAmount = DiscountAmount,
        Notes = Notes,
        AmountPaidNow = AmountPaidNow,
        PaymentMethod = PaymentMethod,
        Lines = Lines.Select(l => new SaleLineDto { ProductId = l.ProductId, Quantity = l.Quantity, UnitRate = l.UnitRate }).ToList()
    };
}

public partial class SaleDialog : Window
{
    public SaleDialogModel Model { get; }
    private readonly Func<string, Task<IReadOnlyList<CustomerSearchResultDto>>> _searchCustomers;
    private readonly Func<int, Task<IReadOnlyList<SiteDto>>> _getSites;

    public SaleDialog(IReadOnlyList<ProductDto> products, string nextInvoiceNumber,
        Func<string, Task<IReadOnlyList<CustomerSearchResultDto>>> searchCustomers,
        Func<int, Task<IReadOnlyList<SiteDto>>> getSites)
    {
        InitializeComponent();
        _searchCustomers = searchCustomers;
        _getSites = getSites;
        Model = new SaleDialogModel { Products = products, InvoiceNumber = nextInvoiceNumber };
        Model.Lines.CollectionChanged += (_, _) => Model.RecalculateTotal();
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
        Model.SelectedCustomerLabel = $"Selected: {customer.FullName} ({customer.Phone})";
        Model.ShowSuggestions = false;
        Model.CustomerSearchText = customer.FullName;

        Model.Sites.Clear();
        foreach (var s in await _getSites(customer.Id)) Model.Sites.Add(s);
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        var row = new SaleLineRow();
        row.PropertyChanged += (_, _) => Model.RecalculateTotal();
        row.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SaleLineRow.ProductId))
            {
                var product = Model.Products.FirstOrDefault(p => p.Id == row.ProductId);
                if (product is not null)
                {
                    row.AvailableStock = product.CurrentStock;
                    row.UnitRate = product.SaleRate;
                }
            }
        };
        Model.Lines.Add(row);
    }

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is SaleLineRow row) Model.Lines.Remove(row);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (Model.CustomerId is null) { Model.ErrorMessage = "Please select a customer."; return; }
        if (string.IsNullOrWhiteSpace(Model.InvoiceNumber)) { Model.ErrorMessage = "Invoice number is required."; return; }
        if (Model.Lines.Count == 0 || Model.Lines.Any(l => l.ProductId == 0 || l.Quantity <= 0))
        { Model.ErrorMessage = "Add at least one valid line item with a product and quantity."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
