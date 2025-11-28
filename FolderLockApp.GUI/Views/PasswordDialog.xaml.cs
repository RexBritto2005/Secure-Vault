using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FolderLockApp.GUI.Views;

public partial class PasswordDialog : Window, INotifyPropertyChanged
{
    private string _errorMessage = string.Empty;
    private Visibility _errorMessageVisibility = Visibility.Collapsed;
    private int _passwordStrength = 0;
    private string _passwordStrengthText = string.Empty;
    private Color _passwordStrengthColor = Colors.Gray;

    public SecureString? Password { get; private set; }
    public string Message { get; set; } = "Please enter your password:";
    public bool RequireConfirmation { get; set; }
    public Visibility ConfirmPasswordVisibility => RequireConfirmation ? Visibility.Visible : Visibility.Collapsed;
    public Visibility PasswordStrengthVisibility => RequireConfirmation ? Visibility.Visible : Visibility.Collapsed;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            ErrorMessageVisibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public Visibility ErrorMessageVisibility
    {
        get => _errorMessageVisibility;
        set
        {
            _errorMessageVisibility = value;
            OnPropertyChanged();
        }
    }

    public int PasswordStrength
    {
        get => _passwordStrength;
        set
        {
            _passwordStrength = value;
            OnPropertyChanged();
        }
    }

    public string PasswordStrengthText
    {
        get => _passwordStrengthText;
        set
        {
            _passwordStrengthText = value;
            OnPropertyChanged();
        }
    }

    public Color PasswordStrengthColor
    {
        get => _passwordStrengthColor;
        set
        {
            _passwordStrengthColor = value;
            OnPropertyChanged();
        }
    }

    public PasswordDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Clear error message when user starts typing
        ErrorMessage = string.Empty;

        // Update password strength indicator if in confirmation mode
        if (RequireConfirmation)
        {
            UpdatePasswordStrength(PasswordBox.Password);
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Clear error message when user starts typing
        ErrorMessage = string.Empty;
    }

    private void UpdatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            PasswordStrength = 0;
            PasswordStrengthText = string.Empty;
            PasswordStrengthColor = Colors.Gray;
            return;
        }

        int strength = 0;

        // Length check
        if (password.Length >= 8) strength++;
        if (password.Length >= 12) strength++;

        // Character variety checks
        if (Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[A-Z]")) strength++;
        if (Regex.IsMatch(password, @"\d")) strength++;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]")) strength++;

        // Cap at 4 for display purposes
        strength = Math.Min(strength, 4);

        PasswordStrength = strength;

        switch (strength)
        {
            case 0:
            case 1:
                PasswordStrengthText = "Weak";
                PasswordStrengthColor = Color.FromRgb(255, 0, 0); // Red
                break;
            case 2:
                PasswordStrengthText = "Fair";
                PasswordStrengthColor = Color.FromRgb(255, 165, 0); // Orange
                break;
            case 3:
                PasswordStrengthText = "Good";
                PasswordStrengthColor = Color.FromRgb(255, 215, 0); // Gold
                break;
            case 4:
                PasswordStrengthText = "Strong";
                PasswordStrengthColor = Color.FromRgb(0, 128, 0); // Green
                break;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
        {
            ErrorMessage = "Password cannot be empty.";
            return;
        }

        if (PasswordBox.Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters long.";
            return;
        }

        if (RequireConfirmation)
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ErrorMessage = "Passwords do not match.";
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Shows an error message in the dialog (e.g., for incorrect password)
    /// </summary>
    public void ShowError(string message)
    {
        ErrorMessage = message;
    }
}
