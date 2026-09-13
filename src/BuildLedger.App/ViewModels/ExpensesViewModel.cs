using BuildLedger.App.Services;
using System.Collections.ObjectModel;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BuildLedger.App.ViewModels;

public partial class ExpensesViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IExpenseService _expenseService;
    public ExpensesViewModel(IExpenseService expenseService) => _expenseService = expenseService;

    public ObservableCollection<ExpenseDto> Expenses { get; } = new();
    public ObservableCollection<ExpenseCategoryDto> Categories { get; } = new();

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        Categories.Clear();
        foreach (var c in await _expenseService.GetCategoriesAsync()) Categories.Add(c);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Expenses.Clear();
        foreach (var e in await _expenseService.SearchAsync(null, null)) Expenses.Add(e);
    }

    [RelayCommand]
    private async Task SaveExpenseAsync(ExpenseUpsertDto dto)
    {
        ClearMessages();
        var result = await _expenseService.UpsertAsync(dto);
        if (!result.Success) { ErrorMessage = result.Error; return; }
        SuccessMessage = "Expense recorded.";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task SaveCategoryAsync(string name)
    {
        await _expenseService.UpsertCategoryAsync(name);
        Categories.Clear();
        foreach (var c in await _expenseService.GetCategoriesAsync()) Categories.Add(c);
    }
}
