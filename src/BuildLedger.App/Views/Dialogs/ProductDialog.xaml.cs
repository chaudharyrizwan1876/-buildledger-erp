using System.Windows;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class ProductDialogModel : ObservableObject
{
    [ObservableProperty] private string title = "New Product";
    [ObservableProperty] private int? id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int categoryId;
    [ObservableProperty] private int unitId;
    [ObservableProperty] private decimal purchaseRate;
    [ObservableProperty] private decimal saleRate;
    [ObservableProperty] private decimal minimumStockLevel;
    [ObservableProperty] private string? description;
    [ObservableProperty] private string? errorMessage;

    public IReadOnlyList<ProductCategoryDto> Categories { get; set; } = Array.Empty<ProductCategoryDto>();
    public IReadOnlyList<UnitDto> Units { get; set; } = Array.Empty<UnitDto>();

    public ProductUpsertDto ToDto() => new()
    {
        Id = Id, Name = Name, CategoryId = CategoryId, UnitId = UnitId,
        PurchaseRate = PurchaseRate, SaleRate = SaleRate, MinimumStockLevel = MinimumStockLevel, Description = Description
    };

    public static ProductDialogModel FromDto(ProductDto p) => new()
    {
        Title = "Edit Product", Id = p.Id, Name = p.Name, CategoryId = p.CategoryId, UnitId = p.UnitId,
        PurchaseRate = p.PurchaseRate, SaleRate = p.SaleRate, MinimumStockLevel = p.MinimumStockLevel, Description = p.Description
    };
}

public partial class ProductDialog : Window
{
    public ProductDialogModel Model { get; }

    public ProductDialog(IReadOnlyList<ProductCategoryDto> categories, IReadOnlyList<UnitDto> units, ProductDialogModel? model = null)
    {
        InitializeComponent();
        Model = model ?? new ProductDialogModel();
        Model.Categories = categories;
        Model.Units = units;
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.Name)) { Model.ErrorMessage = "Product name is required."; return; }
        if (Model.CategoryId == 0 || Model.UnitId == 0) { Model.ErrorMessage = "Category and unit are required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
