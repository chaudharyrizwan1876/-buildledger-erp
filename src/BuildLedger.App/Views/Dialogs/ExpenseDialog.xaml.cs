using System.Windows;
using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class ExpenseDialogModel : ObservableObject
{
    [ObservableProperty] private int categoryId;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private DateTime? expenseDate = DateTime.Now;
    [ObservableProperty] private PaymentMethod method = PaymentMethod.Cash;
    [ObservableProperty] private string? payee;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private string? errorMessage;

    public IReadOnlyList<ExpenseCategoryDto> Categories { get; set; } = Array.Empty<ExpenseCategoryDto>();
    public IReadOnlyList<PaymentMethod> Methods { get; } = Enum.GetValues<PaymentMethod>();

    public ExpenseUpsertDto ToDto() => new()
    { CategoryId = CategoryId, Amount = Amount, ExpenseDate = ExpenseDate ?? DateTime.Now, Method = Method, Payee = Payee, Notes = Notes };
}

public partial class ExpenseDialog : Window
{
    public ExpenseDialogModel Model { get; }

    public ExpenseDialog(IReadOnlyList<ExpenseCategoryDto> categories)
    {
        InitializeComponent();
        Model = new ExpenseDialogModel { Categories = categories };
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (Model.CategoryId == 0) { Model.ErrorMessage = "Please select a category."; return; }
        if (Model.Amount <= 0) { Model.ErrorMessage = "Amount must be greater than zero."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
