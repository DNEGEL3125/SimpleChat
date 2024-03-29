using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleChatServer.Models;
using SimpleChatServer.Persistence;
using Timer = System.Timers.Timer;

namespace SimpleChatServer.Services;

public class VerificationCodeService
{
    private static readonly string Host;
    private static readonly string Password;
    private static readonly string EmailFrom;
    private readonly Timer _cleanupTimer;

    static VerificationCodeService()
    {
        // Get the directory where the application is running from
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Construct the path to your JSON file relative to the root directory
        Debug.Assert(baseDirectory != null, nameof(baseDirectory) + " != null");
        var jsonFilePath = Path.Combine(baseDirectory, "../../..", "ServerConfig.json");

        using var file = File.OpenText(jsonFilePath);
        // Create a JsonTextReader from the file stream
        using var reader = new JsonTextReader(file);
        // Deserialize the JSON data stream directly from the file
        var jsonData = JToken.ReadFrom(reader);
        var smtpInfo = jsonData["Smtp"]
                       ?? throw new InvalidOperationException();
        Password = smtpInfo["Password"]?
            .ToObject<string>() ?? throw new InvalidOperationException();
        Host = smtpInfo["Host"]?
            .ToObject<string>() ?? throw new InvalidOperationException();
        EmailFrom = smtpInfo["EmailFrom"]?
            .ToObject<string>() ?? throw new InvalidOperationException();
    }

    public VerificationCodeService()
    {
        _cleanupTimer = new Timer(TimeSpan.FromSeconds(10));
        _cleanupTimer.Elapsed += CleanupCodeExpired;

        _cleanupTimer.Start();
    }

    ~VerificationCodeService()
    {
        _cleanupTimer.Close();
    }

    private static void CleanupCodeExpired(object? sender, ElapsedEventArgs e)
    {
        VerificationCodeDao.DeleteVerificationCodeBefore(DateTime.Now.AddMinutes(-10));
    }

    public string SendVerificationCode(string? recipientEmail, Header header)
    {
        if (recipientEmail == null)
        {
            return "Network problem, please try again later";
        }

        var isEmailExists = UserDao.GetUserByEmail(recipientEmail) != null;

        switch (header)
        {
            case Header.PasswordResetVerificationCode
                when !isEmailExists:
                return "This email is not registered";
            case Header.RegisterVerificationCode when isEmailExists:
                return "This email has been used";
        }

        var verificationCode = VerificationCodeDao.GetVerificationCodeByEmail(recipientEmail);
        if (verificationCode != null)
        {
            if (DateTime.Now - verificationCode.SendTime < TimeSpan.FromMinutes(1))
            {
                return "The verification code has been sent";
            }

            // Delete the previous code and resend a new one
            VerificationCodeDao.DeleteVerificationCodeByEmail(recipientEmail);
        }

        verificationCode = new VerificationCode { Email = recipientEmail };

        // Create the email message
        var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(EmailFrom);
        mailMessage.To.Add(recipientEmail);
        mailMessage.Subject = "Verification Code";
        mailMessage.IsBodyHtml = true;
        mailMessage.Body =
            $"Your verification code is: <b>{verificationCode}</b><br/>The verification code will expire in 10 minutes";

        var isSuccess = SendEmail(mailMessage);
        if (!isSuccess)
            return "We can't send email to you, please check your email address and try again later";
        VerificationCodeDao.Save(verificationCode);
        return string.Empty;
    }

    private static bool SendEmail(MailMessage mailMessage)
    {
        try
        {
            // Configure the SMTP client
            var client = new SmtpClient(Host)
            {
                Port = 587,
                Credentials = new NetworkCredential(EmailFrom, Password),
                EnableSsl = true
            };

            // Send the email
            client.SendMailAsync(mailMessage);
            // client.Send(mailMessage);

            Console.WriteLine("Verification code sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending verification code: {ex.Message}");
            return false;
        }

        return true;
    }

    public static bool VerifyCode(string email, string code)
    {
        var verificationCodeByEmail = VerificationCodeDao.GetVerificationCodeByEmail(email);
        if (verificationCodeByEmail == null)
        {
            return false;
        }

        if (verificationCodeByEmail.Code != code)
        {
            return false;
        }

        // Delete the verification code after successfully verify
        VerificationCodeDao.DeleteVerificationCodeByEmail(email);
        return true;
    }
}