using System.Windows;
using FolderLockApp.GUI.ViewModels;

namespace FolderLockApp.GUI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadLockedFoldersAsync();
    }
}