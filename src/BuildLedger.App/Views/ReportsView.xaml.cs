using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.Services;
using BuildLedger.App.ViewModels;

namespace BuildLedger.App.Views;

public partial class ReportsView : UserControl
{
    public ReportsView() => InitializeComponent();

    private ReportsViewModel ViewModel => (ReportsViewModel)DataContext;

    private async void PrintCurrentTab_Click(object sender, RoutedEventArgs e)
    {
        var businessName = await ViewModel.GetBusinessNameAsync();
        var range = $"{ViewModel.FromDate:dd MMM yyyy} - {ViewModel.ToDate:dd MMM yyyy}";

        switch (ReportTabs.SelectedIndex)
        {
            case 0:
                InvoicePrintService.PrintTable($"Sales Report ({range})",
                    new[] { "Invoice #", "Date", "Customer", "Total", "Paid", "Due" },
                    ViewModel.SalesReport.Select(s => new[] { s.InvoiceNumber, s.SaleDate.ToString("dd MMM yyyy"), s.CustomerName, s.Total.ToString("N0"), s.AmountPaid.ToString("N0"), s.AmountDue.ToString("N0") }),
                    businessName);
                break;
            case 1:
                InvoicePrintService.PrintTable($"Payments Report ({range})",
                    new[] { "Receipt #", "Date", "Customer", "Amount", "Method" },
                    ViewModel.PaymentsReport.Select(p => new[] { p.ReceiptNumber, p.PaymentDate.ToString("dd MMM yyyy"), p.CustomerName, p.Amount.ToString("N0"), p.Method.ToString() }),
                    businessName);
                break;
            case 2:
                InvoicePrintService.PrintTable("Receivables Report",
                    new[] { "Customer", "Phone", "Balance Owed" },
                    ViewModel.Receivables.Select(r => new[] { r.FullName, r.Phone, r.CurrentBalance.ToString("N0") }),
                    businessName);
                break;
            case 3:
                InvoicePrintService.PrintTable("Supplier Balances Report",
                    new[] { "Supplier", "Phone", "Balance Owed" },
                    ViewModel.SupplierBalances.Select(s => new[] { s.Name, s.Phone ?? "-", s.CurrentBalance.ToString("N0") }),
                    businessName);
                break;
        }
    }
}
