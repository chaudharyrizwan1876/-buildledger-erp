using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class PurchaseLineRow : ObservableObject
{
    [ObservableProperty] private int productId;
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private decimal unitRate;
    public decimal LineTotal => Quantity * UnitRate;

    partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnUnitRateChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
}

public partial class PurchaseDialogModel : ObservableObject
{
    [ObservableProperty] private int supplierId;
    [ObservableProperty] private string invoiceNumber = string.Empty;
    [ObservableProperty] private DateTime? purchaseDate = DateTime.Now;
    [ObservableProperty] private decimal discountAmount;
    [ObservableProperty] private decimal taxAmount;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private string? errorMessage;

    public ObservableCollection<PurchaseLineRow> Lines { get; } = new();
    public IReadOnlyList<SupplierDto> Suppliers { get; set; } = Array.Empty<SupplierDto>();
    public IReadOnlyList<ProductDto> Products { get; set; } = Array.Empty<ProductDto>();

    public decimal SubTotal => Lines.Sum(l => l.LineTotal);
    public decimal Total => SubTotal - DiscountAmount + TaxAmount;

    partial void OnDiscountAmountChanged(decimal value) => OnPropertyChanged(nameof(Total));
    partial void OnTaxAmountChanged(decimal value) => OnPropertyChanged(nameof(Total));

    public void RecalculateTotal() { OnPropertyChanged(nameof(SubTotal)); OnPropertyChanged(nameof(Total)); }

    public CreatePurchaseDto ToDto() => new()
    {
        SupplierId = SupplierId,
        InvoiceNumber = InvoiceNumber,
        PurchaseDate = PurchaseDate ?? DateTime.Now,
        DiscountAmount = DiscountAmount,
        TaxAmount = TaxAmount,
        Notes = Notes,
        Lines = Lines.Select(l => new PurchaseLineDto { ProductId = l.ProductId, Quantity = l.Quantity, UnitRate = l.UnitRate }).ToList()
    };
}

public partial class PurchaseDialog : Window
{
    public PurchaseDialogModel Model { get; }

    public PurchaseDialog(IReadOnlyList<SupplierDto> suppliers, IReadOnlyList<ProductDto> products, string nextInvoiceNumber)
    {
        InitializeComponent();
        Model = new PurchaseDialogModel { Suppliers = suppliers, Products = products, InvoiceNumber = nextInvoiceNumber };
        Model.Lines.CollectionChanged += (_, _) => Model.RecalculateTotal();
        DataContext = Model;
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        var row = new PurchaseLineRow();
        row.PropertyChanged += (_, _) => Model.RecalculateTotal();
        Model.Lines.Add(row);
    }

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is PurchaseLineRow row) Model.Lines.Remove(row);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (Model.SupplierId == 0) { Model.ErrorMessage = "Please select a supplier."; return; }
        if (string.IsNullOrWhiteSpace(Model.InvoiceNumber)) { Model.ErrorMessage = "Invoice number is required."; return; }
        if (Model.Lines.Count == 0 || Model.Lines.Any(l => l.ProductId == 0 || l.Quantity <= 0))
        { Model.ErrorMessage = "Add at least one valid line item with a product and quantity."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
