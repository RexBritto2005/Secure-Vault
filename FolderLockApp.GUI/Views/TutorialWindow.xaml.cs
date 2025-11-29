using System.Windows;

namespace FolderLockApp.GUI.Views;

/// <summary>
/// Tutorial window for first-time users.
/// Implements Requirement 1.1: Quick tutorial for locking first folder
/// </summary>
public partial class TutorialWindow : Window
{
    public TutorialWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
