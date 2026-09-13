using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class InventoryView : UserControl
{
    public InventoryView() => InitializeComponent();

    private InventoryViewModel ViewModel => (InventoryViewModel)DataContext;

    private async void AdjustStock_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new StockAdjustmentDialog(ViewModel.Products.ToList()) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.AdjustStockCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
