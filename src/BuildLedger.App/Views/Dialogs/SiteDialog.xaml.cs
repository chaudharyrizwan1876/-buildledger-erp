using System.Windows;
using BuildLedger.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLedger.App.Views.Dialogs;

public partial class SiteDialogModel : ObservableObject
{
    [ObservableProperty] private string title = "New Site";
    [ObservableProperty] private int? id;
    [ObservableProperty] private int customerId;
    [ObservableProperty] private string siteName = string.Empty;
    [ObservableProperty] private string? projectType;
    [ObservableProperty] private string? contactPerson;
    [ObservableProperty] private string? siteAddress;
    [ObservableProperty] private string? errorMessage;

    public SiteUpsertDto ToDto() => new()
    {
        Id = Id,
        CustomerId = CustomerId,
        SiteName = SiteName,
        ProjectType = ProjectType,
        ContactPerson = ContactPerson,
        SiteAddress = SiteAddress
    };
}

public partial class SiteDialog : Window
{
    public SiteDialogModel Model { get; }

    public SiteDialog(int customerId, SiteDialogModel? model = null)
    {
        InitializeComponent();
        Model = model ?? new SiteDialogModel { CustomerId = customerId };
        DataContext = Model;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Model.ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Model.SiteName)) { Model.ErrorMessage = "Site name is required."; return; }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
