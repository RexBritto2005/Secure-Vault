using System;
using System.Drawing;
using System.Windows.Forms;

namespace FolderLockApp.ShellExtension;

/// <summary>
/// Simple password input dialog for shell extension.
/// </summary>
public class PasswordInputForm : Form
{
    private TextBox passwordTextBox;
    private Button okButton;
    private Button cancelButton;
    private Label messageLabel;

    public string Password => passwordTextBox.Text;

    public PasswordInputForm(string message)
    {
        InitializeComponents(message);
    }

    private void InitializeComponents(string message)
    {
        // Form properties
        Text = "FolderLock";
        Size = new Size(400, 180);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        TopMost = true;

        // Message label
        messageLabel = new Label
        {
            Text = message,
            Location = new Point(20, 20),
            Size = new Size(360, 40),
            AutoSize = false
        };

        // Password textbox
        passwordTextBox = new TextBox
        {
            Location = new Point(20, 70),
            Size = new Size(360, 25),
            UseSystemPasswordChar = true,
            Font = new Font("Segoe UI", 10)
        };

        // OK button
        okButton = new Button
        {
            Text = "OK",
            Location = new Point(200, 110),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        okButton.Click += (s, e) => Close();

        // Cancel button
        cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(290, 110),
            Size = new Size(90, 30),
            DialogResult = DialogResult.Cancel
        };
        cancelButton.Click += (s, e) => Close();

        // Add controls
        Controls.Add(messageLabel);
        Controls.Add(passwordTextBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);

        // Set accept/cancel buttons
        AcceptButton = okButton;
        CancelButton = cancelButton;

        // Focus on password textbox
        passwordTextBox.Select();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            passwordTextBox?.Dispose();
            okButton?.Dispose();
            cancelButton?.Dispose();
            messageLabel?.Dispose();
        }
        base.Dispose(disposing);
    }
}
