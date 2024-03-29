using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleChat.Models;

namespace SimpleChat.Services;

public partial class UserDataService
{
    public static string IsAccountAvailable(User user)
    {
        var tests = new List<Func<string, string>>
        {
            IsUsernameAvailable,
            IsValidEmail,
            IsPasswordAvailable
        };

        var shouldTest = new List<string> { user.Username, user.Email, user.Password };

        if (tests.Count != shouldTest.Count)
        {
            throw new Exception("The length of tests should equal to the length of tested element.");
        }

        for (var i = 0; i < tests.Count; ++i)
        {
            var res = tests[i](shouldTest[i]);
            if (res != string.Empty)
            {
                return res;
            }
        }


        return string.Empty;
    }

    public static string IsUsernameAvailable(string username)
    {
        const int maxUsernameLen = 16;

        if (username.Length > maxUsernameLen)
        {
            return $"Username should not exceed {maxUsernameLen} characters in length";
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username must contain something other than blank space.";
        }


        return string.Empty;
    }

    public static string IsPasswordAvailable(string password)
    {
        const int minPasswordLength = 6;
        const int maxPasswordLength = 32;

        switch (password.Length)
        {
            case > maxPasswordLength:
                return $"Password should not exceed {maxPasswordLength} characters in length";
            case < minPasswordLength:
                return $"Password length should not be less than {minPasswordLength}";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Your password must contain anything else other than white space";
        }

        return string.Empty;
    }

    public static string IsValidEmail(string email)
    {
        const int maxEmailLength = 128;

        if (email.Length > maxEmailLength)
        {
            return $"Email should not exceed {maxEmailLength} characters in length";
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return "Your email must contain anything else other than white space";
        }

        if (!EmailRegex().IsMatch(email))
        {
            return "This is not a valid email format";
        }

        return string.Empty;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}