using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SimpleChat.Services;
using SimpleChat.ViewModels;

namespace SimpleChat.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void GotFocusHandler(object? sender, GotFocusEventArgs e)
    {
        var source = e.Source as Control;
        if (DataContext is not RegisterViewModel vm)
        {
            return;
        }

        switch (source?.Name)
        {
            case "EmailInput":
                vm.EmailTip = string.Empty;
                break;
            case "UsernameInput":
                vm.UsernameTip = string.Empty;
                break;
            case "PasswordInput":
                vm.PasswordTip = string.Empty;
                break;
        }
    }

    private void LostFocusHandler(object? sender, RoutedEventArgs e)
    {
        var source = e.Source as Control;
        if (DataContext is not RegisterViewModel vm)
        {
            return;
        }

        switch (source?.Name)
        {
            case "EmailInput":
                var emailInput = source as TextBox;
                if (string.IsNullOrEmpty(emailInput?.Text))
                {
                    break;
                }

                vm.EmailTip = UserDataService.IsValidEmail(emailInput.Text);
                break;
            case "UsernameInput":
                var usernameInput = source as TextBox;
                if (string.IsNullOrEmpty(usernameInput?.Text))
                {
                    break;
                }

                vm.UsernameTip = UserDataService.IsUsernameAvailable(usernameInput.Text);
                break;
            case "PasswordInput":
                var passwordInput = source as TextBox;
                if (string.IsNullOrEmpty(passwordInput?.Text))
                {
                    break;
                }

                vm.PasswordTip = UserDataService.IsPasswordAvailable(passwordInput.Text);
                break;
            case "CancelButton":
                // do something ...
                break;
        }

        e.Handled = true;
    }
}