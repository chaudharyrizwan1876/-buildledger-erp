using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class SimpleInputDialogModel : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string field1Label = string.Empty;
    [ObservableProperty] private string field1Value = string.Empty;
    [ObservableProperty] private bool hasField2;
    [ObservableProperty] private string field2Label = string.Empty;
    [ObservableProperty] private string field2Value = string.Empty;
    [ObservableProperty] private string? errorMessage;
}

public partial class SimpleInputDialog : Window
{
    public SimpleInputDialogModel Model { get; }

    public SimpleInputDialog(SimpleInputDialogModel model)
    {
        InitializeComponent();
        Model = model;
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.Field1Value)) { Model.ErrorMessage = $"{Model.Field1Label} is required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
