using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleChat.Models;
using SimpleChat.Services;

namespace SimpleChat.ViewModels;

public class PasswordResetViewModel : ViewModelBase
{
    private string _passwordTip = string.Empty;
    private string _retypePasswordTip = string.Empty;
    private string _emailTip = string.Empty;

    private string _password = string.Empty;
    private string _retypePassword = string.Empty;
    private string _email = string.Empty;
    private string _verificationCode = string.Empty;
    private string _verificationCodeButtonContent = "Send code";
    private string _verificationCodeTip = string.Empty;
    private readonly UserDataService _userDataService = new();

    private bool _canGetVerificationCode = true;
    private int _verificationCodeCooldownSeconds;

    public ReactiveCommand<Unit, string> GetVerificationCodeCommand { get; }
    public ReactiveCommand<Unit, PasswordResetForm> PasswordResetCommand { get; }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public PasswordResetViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.Email,
            x => x.Password,
            x => x.VerificationCode,
            x => x.RetypePassword,
            (email, password, verificationCode, retypePassword) =>
                UserDataService.IsAccountAvailable(new User
                    { Email = email, Password = password, Username = "Unknown" }) == string.Empty &&
                !string.IsNullOrEmpty(verificationCode) && password == retypePassword);

        PasswordResetCommand = ReactiveCommand.Create(
            () => new PasswordResetForm
                { Email = Email, Password = Password, VerificationCode = VerificationCode },
            isValidObservable);

        isValidObservable = this.WhenAnyValue(
            x => x.Email,
            (email) =>
                UserDataService.IsValidEmail(email) == string.Empty
        );

        GetVerificationCodeCommand = ReactiveCommand.Create(
            () => Email, isValidObservable
        );

        LoginCommand = ReactiveCommand.Create(() => { });
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

    public string PasswordTip
    {
        get => _passwordTip;
        set => this.RaiseAndSetIfChanged(ref _passwordTip, value);
    }

    public string RetypePasswordTip
    {
        get => _retypePasswordTip;
        set => this.RaiseAndSetIfChanged(ref _retypePasswordTip, value);
    }

    public string EmailTip
    {
        get => _emailTip;
        set => this.RaiseAndSetIfChanged(ref _emailTip, value);
    }

    public string VerificationCodeTip
    {
        get => _verificationCodeTip;
        set => this.RaiseAndSetIfChanged(ref _verificationCodeTip, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string RetypePassword
    {
        get => _retypePassword;
        set => this.RaiseAndSetIfChanged(ref _retypePassword, value);
    }

    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public string VerificationCode
    {
        get => _verificationCode;
        set => this.RaiseAndSetIfChanged(ref _verificationCode, value);
    }

    public string VerificationCodeButtonContent
    {
        get => _verificationCodeButtonContent;
        set => this.RaiseAndSetIfChanged(ref _verificationCodeButtonContent, value);
    }

    public bool CanGetVerificationCode
    {
        get => _canGetVerificationCode;
        set => this.RaiseAndSetIfChanged(ref _canGetVerificationCode, value);
    }
}