using System.Windows;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class SupplierDialogModel : ObservableObject
{
    [ObservableProperty] private string title = "New Supplier";
    [ObservableProperty] private bool isNew = true;
    [ObservableProperty] private int? id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string? phone;
    [ObservableProperty] private string? address;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private decimal openingBalance;
    [ObservableProperty] private string? errorMessage;

    public SupplierUpsertDto ToDto() => new()
    { Id = Id, Name = Name, Phone = Phone, Address = Address, Notes = Notes, OpeningBalance = OpeningBalance };

    public static SupplierDialogModel FromDto(SupplierDto s) => new()
    { Title = "Edit Supplier", IsNew = false, Id = s.Id, Name = s.Name, Phone = s.Phone, Address = s.Address };
}

public partial class SupplierDialog : Window
{
    public SupplierDialogModel Model { get; }
    public SupplierDialog(SupplierDialogModel? model = null)
    {
        InitializeComponent();
        Model = model ?? new SupplierDialogModel();
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.Name)) { Model.ErrorMessage = "Supplier name is required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
