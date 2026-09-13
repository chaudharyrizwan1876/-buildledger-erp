using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class SalesView : UserControl
{
    public SalesView() => InitializeComponent();

    private SalesViewModel ViewModel => (SalesViewModel)DataContext;

    private async void NewSale_Click(object sender, RoutedEventArgs e)
    {
        var nextInvoice = await ViewModel.GenerateNextInvoiceNumberAsync();
        var dialog = new SaleDialog(ViewModel.Products.ToList(), nextInvoice, ViewModel.SearchCustomersAsync, ViewModel.GetSitesAsync)
        { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.CreateSaleCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
