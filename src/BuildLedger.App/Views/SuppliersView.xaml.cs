using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;
using BuildLedger.Application.DTOs;

namespace BuildLedger.App.Views;

public partial class SuppliersView : UserControl
{
    public SuppliersView() => InitializeComponent();

    private SuppliersViewModel ViewModel => (SuppliersViewModel)DataContext;

    private async void NewSupplier_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SupplierDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveSupplierCommand.ExecuteAsync(dialog.Model.ToDto());
    }

    private async void EditSupplier_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not SupplierDto supplier) return;
        var dialog = new SupplierDialog(SupplierDialogModel.FromDto(supplier)) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveSupplierCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
