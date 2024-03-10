using Search_Algorithms.Games.Shortest_Path;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Search_Algorithms.Algorithms;
using Search_Algorithms;
using System.Globalization;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace Form;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        if (WindowState == WindowState.Maximized) Window_StateChanged(new object(), new EventArgs());
    }

    private async void Window_StateChanged(object sender, EventArgs e)
    {
        //if (WindowState.Maximized == this.WindowState)
        //{
        //    await Task.Run(() =>
        //    {
        //        Thread.Sleep(1);
        //        Dispatcher.Invoke(() =>
        //        {
        //            Grid.Width = Grid.ActualHeight;
        //        });
        //    });
        //}
        //else if (WindowState == WindowState.Normal)
        //{
        //    await Task.Run(() =>
        //    {
        //        Thread.Sleep(1);
        //        Dispatcher.Invoke(() =>
        //        {
        //            Grid.Width = Grid.ActualHeight;
        //        });
        //    });
        //}
    }

}