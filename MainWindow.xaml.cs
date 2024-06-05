using AlwaysGetUp.ViewModels;
using System.Windows;

namespace AlwaysGetUp;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel = MainViewModel.Instance;
    public MainWindow()
    {
        DataContext = viewModel;
        viewModel.OneHourOfSittingEvent += HandleOneHourOfSitting;
        Left = 100;
        Top = 1250;
        Deactivated += HandleWindowLostFocus;
    }

    private void HandleWindowLostFocus(object sender, EventArgs e)
    {
        if (viewModel.isWorkPeriodTaskActive)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
            }
        }
    }

    private void HandleOneHourOfSitting(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
        BringIntoView();
        Focus();
    }
}