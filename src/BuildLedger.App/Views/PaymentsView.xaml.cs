using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class PaymentsView : UserControl
{
    public PaymentsView() => InitializeComponent();

    private PaymentsViewModel ViewModel => (PaymentsViewModel)DataContext;

    private async void NewPayment_Click(object sender, RoutedEventArgs e)
    {
        var nextReceipt = await ViewModel.GenerateNextReceiptNumberAsync();
        var dialog = new PaymentDialog(nextReceipt, ViewModel.SearchCustomersAsync, ViewModel.GetCustomerSalesAsync)
        { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.RecordPaymentCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
