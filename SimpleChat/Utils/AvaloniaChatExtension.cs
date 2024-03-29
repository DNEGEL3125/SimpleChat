using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using SimpleChat.Models;


namespace SimpleChat.Utils;

public static class AvaloniaChatExtension
{
    public static Window ShowMessageBox(string text, string title = "", double width = 200, double height = 150)
    {
        // Get the MainWindow
        var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        var w = new Window
        {
            Title = title,
            MinHeight = height,
            MinWidth = width,
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = new TextBlock { TextWrapping = TextWrapping.Wrap, Text = text },
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };

        // Create a Grid for layout
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        // Add a TextBlock
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = text,
            Margin = new Thickness(10),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(textBlock, 0);
        Grid.SetColumn(textBlock, 0);
        grid.Children.Add(textBlock);

        // Add a Button
        var closeButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10),
            Padding = new Thickness(10, 2),
            FontSize = 16
        };
        closeButton.Click += (_, _) => w.Close();
        Grid.SetRow(closeButton, 1);
        Grid.SetColumn(closeButton, 0);
        grid.Children.Add(closeButton);

        // Set the Grid as the content of the Window
        w.Content = grid;


        if (mainWindow != null)
        {
            // disable the parent window
            mainWindow.IsEnabled = false;
            // enable the parent window again after w is closed
            w.Closed += (_, _) => mainWindow.IsEnabled = true;
            w.ShowDialog(mainWindow);
        }
        else w.Show();

        return w;
    }

    public static void SaveChatImage(ChatImage chatImage)
    {
        var filePath = chatImage.FilePath;
        var imageData = Convert.FromBase64String(chatImage.Base64Data);
        // Create the directory if it doesn't exist
        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath) ?? string.Empty
        );
        // SaveAndGetId file to directory
        File.WriteAllBytes(filePath, imageData);
    }
}