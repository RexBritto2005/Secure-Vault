using System.Security;
using System.Windows;

namespace FolderLockApp.GUI.Views;

public partial class PasswordDialog : Window
{
    public SecureString? Password { get; private set; }
    public string Message { get; set; } = "Please enter your password:";
    public bool RequireConfirmation { get; set; }
    public Visibility ConfirmPasswordVisibility => RequireConfirmation ? Visibility.Visible : Visibility.Collapsed;

    public PasswordDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Password is handled securely through the PasswordBox control
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Confirmation password is handled securely through the PasswordBox control
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
        {
            MessageBox.Show("Password cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (PasswordBox.Password.Length < 8)
        {
            MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RequireConfirmation)
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Convert to SecureString
        Password = new SecureString();
        foreach (char c in PasswordBox.Password)
        {
            Password.AppendChar(c);
        }
        Password.MakeReadOnly();

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
