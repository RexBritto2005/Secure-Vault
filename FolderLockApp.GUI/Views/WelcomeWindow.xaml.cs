using System.Windows;

namespace FolderLockApp.GUI.Views;

/// <summary>
/// Welcome window shown on first run.
/// Implements Requirement 1.1: First-run experience
/// </summary>
public partial class WelcomeWindow : Window
{
    public bool ShowTutorial { get; private set; }
    public bool DontShowAgain { get; private set; }

    public WelcomeWindow()
    {
        InitializeComponent();
    }

    private void GetStartedButton_Click(object sender, RoutedEventArgs e)
    {
        DontShowAgain = DontShowAgainCheckBox.IsChecked ?? false;
        ShowTutorial = false;
        DialogResult = true;
        Close();
    }

    private void TutorialButton_Click(object sender, RoutedEventArgs e)
    {
        DontShowAgain = DontShowAgainCheckBox.IsChecked ?? false;
        ShowTutorial = true;
        DialogResult = true;
        Close();
    }
}
