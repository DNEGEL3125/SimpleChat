using System;
using System.Reactive;
using ReactiveUI;
using SimpleChat.Models;

namespace SimpleChat.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _email = string.Empty;
    private string _password = string.Empty;
    public ReactiveCommand<Unit, User> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> PasswordResetCommand { get; }

    public LoginViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.Email,
            x => x.Password,
            (email, password) =>
                !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password));

        LoginCommand = ReactiveCommand.Create(
            () => new User { Email = Email, Password = Password },
            isValidObservable);

        RegisterCommand = ReactiveCommand.Create(() => { Console.WriteLine("login to register");});

        PasswordResetCommand = ReactiveCommand.Create(() => { });
    }

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
}