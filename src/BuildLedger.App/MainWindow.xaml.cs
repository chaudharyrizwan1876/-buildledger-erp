using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BuildLedger.App.ViewModels;

namespace BuildLedger.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var contentProperty = DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl));
        contentProperty?.AddValueChanged(ContentHost, (_, _) => AnimateContentTransition());

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

    private void AnimateContentTransition()
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220));
        var slideIn = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        ContentHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        ContentSlide.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideIn);
    }
}
