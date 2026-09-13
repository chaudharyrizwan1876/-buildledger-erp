using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class PurchasesView : UserControl
{
    public PurchasesView() => InitializeComponent();

    private PurchasesViewModel ViewModel => (PurchasesViewModel)DataContext;

    private async void NewPurchase_Click(object sender, RoutedEventArgs e)
    {
        var nextInvoice = await ViewModel.GenerateNextInvoiceNumberAsync();
        var dialog = new PurchaseDialog(ViewModel.Suppliers.ToList(), ViewModel.Products.ToList(), nextInvoice)
        { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.CreatePurchaseCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
