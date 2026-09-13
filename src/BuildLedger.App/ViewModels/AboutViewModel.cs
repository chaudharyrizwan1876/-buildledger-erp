namespace BuildLedger.App.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    public string AppVersion => "1.0.0";
    public string Framework => ".NET 10 (WPF, EF Core, SQLite)";
}
