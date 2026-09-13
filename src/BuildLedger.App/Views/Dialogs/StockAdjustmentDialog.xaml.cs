using System.Windows;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class StockAdjustmentDialogModel : ObservableObject
{
    [ObservableProperty] private int productId;
    [ObservableProperty] private decimal newQuantity;
    [ObservableProperty] private string reason = string.Empty;
    [ObservableProperty] private string? errorMessage;
    public IReadOnlyList<ProductDto> Products { get; set; } = Array.Empty<ProductDto>();

    public string CurrentStockLabel => Products.FirstOrDefault(p => p.Id == ProductId) is { } p ? $"{p.CurrentStock:N2} {p.UnitAbbreviation}" : "—";
    partial void OnProductIdChanged(int value) => OnPropertyChanged(nameof(CurrentStockLabel));

    public StockAdjustmentDto ToDto() => new() { ProductId = ProductId, NewQuantity = NewQuantity, Reason = Reason };
}

public partial class StockAdjustmentDialog : Window
{
    public StockAdjustmentDialogModel Model { get; }

    public StockAdjustmentDialog(IReadOnlyList<ProductDto> products)
    {
        InitializeComponent();
        Model = new StockAdjustmentDialogModel { Products = products };
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (Model.ProductId == 0) { Model.ErrorMessage = "Please select a product."; return; }
        if (string.IsNullOrWhiteSpace(Model.Reason)) { Model.ErrorMessage = "A reason is required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
