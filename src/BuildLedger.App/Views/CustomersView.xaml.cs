using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
    }

    private CustomersViewModel ViewModel => (CustomersViewModel)DataContext;

    private async void NewCustomer_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CustomerDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await ViewModel.SaveCustomerCommand.ExecuteAsync(dialog.Model.ToDto());
        }
    }

    private async void EditCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCustomerDetail is null) return;
        var model = CustomerDialogModel.FromDetail(ViewModel.SelectedCustomerDetail);
        var dialog = new CustomerDialog(model) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await ViewModel.SaveCustomerCommand.ExecuteAsync(dialog.Model.ToDto());
        }
    }

    private async void NewSite_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCustomerDetail is null) return;
        var dialog = new SiteDialog(ViewModel.SelectedCustomerDetail.Id) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await ViewModel.SaveSiteCommand.ExecuteAsync(dialog.Model.ToDto());
        }
    }
}
