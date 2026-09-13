using System.Windows;
using System.Windows.Controls;
using BuildLedger.App.ViewModels;
using BuildLedger.App.Views.Dialogs;

namespace BuildLedger.App.Views;

public partial class ExpensesView : UserControl
{
    public ExpensesView() => InitializeComponent();

    private ExpensesViewModel ViewModel => (ExpensesViewModel)DataContext;

    private async void NewExpense_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ExpenseDialog(ViewModel.Categories.ToList()) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await ViewModel.SaveExpenseCommand.ExecuteAsync(dialog.Model.ToDto());
    }
}
