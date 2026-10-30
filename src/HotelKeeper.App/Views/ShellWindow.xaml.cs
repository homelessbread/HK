using System.Windows;
using System.Windows.Controls;
using HotelKeeper.Models;

namespace HotelKeeper.App.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
        DataContextChanged += ShellWindow_DataContextChanged;
    }

    private void ShellWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is HotelKeeper.ViewModels.ShellViewModel vm && vm.Session?.Role == Roles.Guest)
        {
            if (FindName("SidebarBorder") is FrameworkElement sidebar)
                sidebar.Visibility = Visibility.Collapsed;
            if (FindName("MainGridColumn") is ColumnDefinition col)
                col.Width = new GridLength(0);
        }
    }
}
