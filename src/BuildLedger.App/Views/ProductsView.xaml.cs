using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;
using BuildLedger.Application.DTOs;

namespace BuildLedger.App.Views;

public partial class ProductsView : UserControl
{
    public ProductsView()
    {
        InitializeComponent();
    }

    private ProductsViewModel ViewModel => (ProductsViewModel)DataContext;

    private async void NewProduct_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProductDialog(ViewModel.Categories.ToList(), ViewModel.Units.ToList()) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveProductCommand.ExecuteAsync(dialog.Model.ToDto());
    }

    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not ProductDto product) return;
        var model = ProductDialogModel.FromDto(product);
        var dialog = new ProductDialog(ViewModel.Categories.ToList(), ViewModel.Units.ToList(), model) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveProductCommand.ExecuteAsync(dialog.Model.ToDto());
    }

    private async void NewCategory_Click(object sender, RoutedEventArgs e)
    {
        var model = new SimpleInputDialogModel { Title = "New Category", Field1Label = "Category Name" };
        var dialog = new SimpleInputDialog(model) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveCategoryCommand.ExecuteAsync(new ProductCategoryUpsertDto { Name = model.Field1Value });
    }

    private async void NewUnit_Click(object sender, RoutedEventArgs e)
    {
        var model = new SimpleInputDialogModel { Title = "New Unit", Field1Label = "Unit Name", HasField2 = true, Field2Label = "Abbreviation" };
        var dialog = new SimpleInputDialog(model) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveUnitCommand.ExecuteAsync(new UnitUpsertDto { Name = model.Field1Value, Abbreviation = model.Field2Value });
    }
}
