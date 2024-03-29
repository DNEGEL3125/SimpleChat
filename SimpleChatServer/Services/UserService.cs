using System.Text.RegularExpressions;
using SimpleChatServer.Models;
using SimpleChatServer.Persistence;
using SimpleChatServer.Utils;

namespace SimpleChatServer.Services;

public static class UserService
{
    private static string IsValidUsername(string username)
    {
        const int maxUsernameLen = 16;
        if (username.Length > maxUsernameLen)
        {
            return $"The length of the username should be less than or equal to {maxUsernameLen}";
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return "The username cannot consist solely of spaces";
        }

        return string.Empty;
    }

    private static string IsValidPassword(string password)
    {
        const int minPasswordLength = 6;

        if (password.Length > 32)
        {
            return "Password is too long";
        }

        if (password.Length < minPasswordLength)
        {
            return $"Password should have at least {minPasswordLength} characters";
        }

        return string.Empty;
    }

    private static string IsValidEmail(string email)
    {
        if (email.Length > 128)
        {
            return "Email address is too long";
        }

        if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
        {
            return "The format of email is invalid";
        }

        return string.Empty;
    }


    public static User? LogIn(User? account)
    {
        if (account == null)
        {
            return null;
        }

        if (IsValidEmail(account.Email) != string.Empty)
        {
            return null;
        }

        var foundAccount = UserDao.GetUserByEmail(account.Email);
        if (foundAccount == null)
        {
            return null;
        }

        var passwordSalt = Convert.FromBase64String(foundAccount.PasswordSalt);
        var hashedPassword = Encryptor.HashPassword(account.Password, passwordSalt);

        account.Username = foundAccount.Username;

        return foundAccount.HashedPassword == hashedPassword
            ? foundAccount.ToUser()
            : null;
    }

    public static string PasswordReset(PasswordResetForm? passwordResetForm)
    {
        if (passwordResetForm == null)
        {
            return "Network problem, please try again later";
        }

        var res = IsValidEmail(passwordResetForm.Email);
        if (res != string.Empty)
        {
            return res;
        }

        if (UserDao.GetUserByEmail(passwordResetForm.Email) == null)
        {
            return "This email is not registered";
        }

        res = IsValidPassword(passwordResetForm.Password);
        if (res != string.Empty)
        {
            return res;
        }

        if (VerificationCodeService.VerifyCode(
                passwordResetForm.Email, passwordResetForm.VerificationCode
            ) != true)
        {
            return "Wrong verification code";
        }

        UserDao.UpdatePasswordByEmail(passwordResetForm.ToSavedUser());
        return string.Empty;
    }

    public static RegisterInfo Register(RegisterForm? registerForm)
    {
        if (registerForm == null)
        {
            return new RegisterInfo { Account = null, Msg = "Network problem, please try again later" };
        }

        var res = IsValidUsername(registerForm.Username);
        if (res != string.Empty)
        {
            return new RegisterInfo { Account = null, Msg = res };
        }

        res = IsValidPassword(registerForm.Password);
        if (res != string.Empty)
        {
            return new RegisterInfo { Account = null, Msg = res };
        }

        res = IsValidEmail(registerForm.Email);
        if (res != string.Empty)
        {
            return new RegisterInfo { Account = null, Msg = res };
        }

        // Duplicate email
        if (UserDao.GetUserByEmail(registerForm.Email) != null)
        {
            return new RegisterInfo { Account = null, Msg = "This email address has been used" };
        }

        if (VerificationCodeService.VerifyCode(
                registerForm.Email,
                registerForm.VerificationCode
            ) != true)
        {
            return new RegisterInfo
            {
                Account = null,
                Msg = "Wrong verification code"
            };
        }

        var account = UserDao.Save(registerForm.ToSavedUser())
            .ToUser();

        return new RegisterInfo
        {
            Account = account, Msg = res
        };
    }

    public static SearchFriendAndGroupChatRes SearchFriendAndGroupChat(SearchFriendAndGroupChatForm? form)
    {
        var res = new SearchFriendAndGroupChatRes();
        if (form == null)
        {
            return res;
        }

        if (form.IsSearchGroupChat)
            res.MatchingChats = SearchGroupChat(form.Keyword);
        if (form.IsSearchFriend)
            res.MatchingUsers = SearchFriend(form.Keyword);
        return res;
    }

    private static IEnumerable<Chat> SearchGroupChat(string? keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return new List<Chat>();
        }

        return ChatDao.SearchChatsByTitle(keyword);
    }

    private static IEnumerable<User> SearchFriend(string? keyword)
    {
        var resultList = new List<User>();
        if (string.IsNullOrEmpty(keyword))
        {
            return resultList;
        }

        if (IsValidEmail(keyword) == string.Empty)
        {
            var user = UserDao.GetUserByEmail(keyword)
                ?.ToUser();
            if (user != null)
                resultList.Add(user);
        }

        resultList.AddRange(
            UserDao.SearchUserByUsername(keyword)
                .Select(x => x.ToUser())
        );

        return resultList;
    }
}