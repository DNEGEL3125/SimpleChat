using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleChat.Models;
using SimpleChat.Services;

namespace SimpleChat.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly UserDataService _userDataService = new();
    private int _verificationCodeCooldownSeconds;
    private bool _canGetVerificationCode = true;
    public ReactiveCommand<Unit, RegisterForm> RegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, string> GetVerificationCodeCommand { get; }

    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _verificationCode = string.Empty;

    private string _emailTip = string.Empty;
    private string _usernameTip = string.Empty;
    private string _passwordTip = string.Empty;
    private string _verificationCodeTip = string.Empty;

    private string _verificationCodeButtonContent = "Send code";

    public RegisterViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.Username,
            x => x.Email,
            x => x.Password,
            x => x.VerificationCode,
            (username, email, password, verificationCode) =>
                UserDataService.IsAccountAvailable(new User
                    { Username = username, Email = email, Password = password }) == string.Empty &&
                !string.IsNullOrEmpty(verificationCode));

        RegisterCommand = ReactiveCommand.Create(
            () => new RegisterForm
                { Username = Username, Email = Email, Password = Password, VerificationCode = VerificationCode },
            isValidObservable);

        LoginCommand = ReactiveCommand.Create(() => { });


        isValidObservable = this.WhenAnyValue(
            x => x.Email,
            (email) =>
                UserDataService.IsValidEmail(email) == string.Empty
        );

        GetVerificationCodeCommand = ReactiveCommand.Create(
            () => Email, isValidObservable
        );
    }

    public void ClearVerificationCodeCooldown()
    {
        _verificationCodeCooldownSeconds = 1;
    }

    public void ResetVerificationCodeCooldown()
    {
        _verificationCodeCooldownSeconds = 60;
        CanGetVerificationCode = false;
        Task.Run(CountDownCooldown);
    }

    private async Task CountDownCooldown()
    {
        // Start countdown
        while (_verificationCodeCooldownSeconds > 0)
        {
            VerificationCodeButtonContent = $"Resend ({_verificationCodeCooldownSeconds})";
            --_verificationCodeCooldownSeconds;

            await Task.Delay(1000);
        }

        CanGetVerificationCode = true;
        VerificationCodeButtonContent = "Send code";
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
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

    public string VerificationCode
    {
        get => _verificationCode;
        set => this.RaiseAndSetIfChanged(ref _verificationCode, value);
    }

    public string EmailTip
    {
        get => _emailTip;
        set => this.RaiseAndSetIfChanged(ref _emailTip, value);
    }

    public string UsernameTip
    {
        get => _usernameTip;
        set => this.RaiseAndSetIfChanged(ref _usernameTip, value);
    }

    public string PasswordTip
    {
        get => _passwordTip;
        set => this.RaiseAndSetIfChanged(ref _passwordTip, value);
    }

    public string VerificationCodeTip
    {
        get => _verificationCodeTip;
        set => this.RaiseAndSetIfChanged(ref _verificationCodeTip, value);
    }

    public bool CanGetVerificationCode
    {
        get => _canGetVerificationCode;
        set => this.RaiseAndSetIfChanged(ref _canGetVerificationCode, value);
    }

    public string VerificationCodeButtonContent
    {
        get => _verificationCodeButtonContent;
        set => this.RaiseAndSetIfChanged(ref _verificationCodeButtonContent, value);
    }
}