using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using SimpleChat.Models;

namespace SimpleChat.Converters;

public class ChatMessageIsSelfAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        object left;
        object right;
        if (targetType == typeof(TextAlignment))
        {
            left = TextAlignment.Left;
            right = TextAlignment.Right;
        }
        else if (targetType == typeof(HorizontalAlignment))
        {
            left = HorizontalAlignment.Left;
            right = HorizontalAlignment.Right;
        }
        else
        {
            throw new Exception($"Unknown targetType {targetType}");
        }

        var sender = (value as ChatMessage)?.Sender;
        if (sender == null)
        {
            // Assume that current user is not "null"
            return left;
        }

        return sender.IsSelf ? right : left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var ali = value as Enum;
        return Equals(ali, HorizontalAlignment.Right);
    }
}