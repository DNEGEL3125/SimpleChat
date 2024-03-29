using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SimpleChat.Services;
using SimpleChat.ViewModels;

namespace SimpleChat.Views;

public partial class PasswordResetView : UserControl
{
    public PasswordResetView()
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
        if (DataContext is not PasswordResetViewModel vm)
        {
            return;
        }

        switch (source?.Name)
        {
            case "EmailInput":
                vm.EmailTip = string.Empty;
                break;
            case "PasswordInput":
                vm.PasswordTip = string.Empty;
                break;
            case "RetypePasswordInput":
                vm.RetypePasswordTip = string.Empty;
                break;
        }

        e.Handled = true;
    }

    private void LostFocusHandler(object? sender, RoutedEventArgs e)
    {
        var source = e.Source as Control;
        if (DataContext is not PasswordResetViewModel vm)
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
            case "PasswordInput":
                var passwordInput = source as TextBox;
                if (string.IsNullOrEmpty(passwordInput?.Text))
                {
                    break;
                }

                vm.PasswordTip = UserDataService.IsPasswordAvailable(passwordInput.Text);
                break;

            case "RetypePasswordInput":
                var retypePasswordInput = source as TextBox;
                if (string.IsNullOrEmpty(retypePasswordInput?.Text))
                {
                    break;
                }

                vm.RetypePasswordTip = vm.Password == vm.RetypePassword
                    ? string.Empty
                    : "Password confirmation doesn't match Password";
                break;
        }

        e.Handled = true;
    }
}