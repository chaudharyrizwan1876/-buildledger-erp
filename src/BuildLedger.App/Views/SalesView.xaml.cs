using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.Services;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;
using BuildLedger.Application.DTOs;

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

    private async void PrintSale_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not SaleSummaryDto sale) return;
        var detail = await ViewModel.GetSaleDetailAsync(sale.Id);
        if (detail is null) return;
        var businessName = await ViewModel.GetBusinessNameAsync();
        InvoicePrintService.PrintSaleInvoice(detail, businessName);
    }
}
