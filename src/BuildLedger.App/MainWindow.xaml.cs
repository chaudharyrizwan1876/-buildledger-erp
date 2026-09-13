using System.Windows;
using BuildLedger.App.ViewModels;

namespace BuildLedger.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is MainViewModel vm)
            {
                vm.SignedOut += () =>
                {
                    Close();
                    ((App)System.Windows.Application.Current).ShowLogin();
                };
            }
        };
    }
}
