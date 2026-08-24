using System;
using System.Windows;
using NRTX.Optimizer.Gui.ViewModels;

namespace NRTX.Optimizer.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += (s, e) =>
        {
            try
            {
                Application.Current.Shutdown(0);
            }
            catch { }
        };
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
