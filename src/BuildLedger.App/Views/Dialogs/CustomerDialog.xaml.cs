using System.Windows;
using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class CustomerDialogModel : ObservableObject
{
    [ObservableProperty] private string title = "New Customer";
    [ObservableProperty] private bool isNew = true;
    [ObservableProperty] private int? id;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string? alternatePhone;
    [ObservableProperty] private string? address;
    [ObservableProperty] private CustomerType customerType = CustomerType.Individual;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private decimal openingBalance;
    [ObservableProperty] private string? errorMessage;

    public IReadOnlyList<CustomerType> CustomerTypes { get; } = Enum.GetValues<CustomerType>();

    public CustomerUpsertDto ToDto() => new()
    {
        Id = Id,
        FullName = FullName,
        Phone = Phone,
        AlternatePhone = AlternatePhone,
        Address = Address,
        CustomerType = CustomerType,
        Notes = Notes,
        OpeningBalance = OpeningBalance
    };

    public static CustomerDialogModel FromDetail(CustomerDetailDto detail) => new()
    {
        Title = "Edit Customer",
        IsNew = false,
        Id = detail.Id,
        FullName = detail.FullName,
        Phone = detail.Phone,
        AlternatePhone = detail.AlternatePhone,
        Address = detail.Address,
        CustomerType = detail.CustomerType,
        Notes = detail.Notes,
        OpeningBalance = detail.OpeningBalance
    };
}

public partial class CustomerDialog : Window
{
    public CustomerDialogModel Model { get; }

    public CustomerDialog(CustomerDialogModel? model = null)
    {
        InitializeComponent();
        Model = model ?? new CustomerDialogModel();
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.FullName)) { Model.ErrorMessage = "Full name is required."; return; }
        if (string.IsNullOrWhiteSpace(Model.Phone)) { Model.ErrorMessage = "Phone number is required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
