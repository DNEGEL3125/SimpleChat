using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleChat.ViewModels;

namespace SimpleChat.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void OnTextChanging(object sender, TextChangingEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        if (DataContext is not ChatViewModel vm)
        {
            return;
        }

        var maxLength = ChatViewModel.MaxMessageLen;
        if (textBox.Text == null || textBox.Text.Length <= maxLength) return;
        // Remove text out of range
        textBox.Text = textBox.Text[..maxLength];
        // Show bubble tip
        vm.BubbleTipOpacity = 1;
        vm.BubbleTipText = $"Maximum length exceeded ({maxLength} characters)";
    }

    private int _lastListMessagesLength = -1;

    private void MessagesScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer messageScrollViewer)
        {
            return;
        }

        if (DataContext is not ChatViewModel vm)
        {
            return;
        }

        // Console.WriteLine("Offset.Y = {0}", messageScrollViewer.Offset.Y);
        Console.WriteLine("ExtentDelta.Y = {0}", e.ExtentDelta.Y);
        Console.WriteLine("OffsetDelta.Y = {0}", e.OffsetDelta.Y);
        Console.WriteLine("ViewportDelta.Y = {0}", e.ViewportDelta.Y);

        // Determine whether size change is happening at bottom or top
        if (Math.Abs(e.OffsetDelta.Y + messageScrollViewer.Viewport.Height - e.ExtentDelta.Y) < 1)
        {
            // Length changing is happening at the bottom
            Console.WriteLine("Size changing is happening at the bottom");
        }
        else if (Math.Abs(e.ExtentDelta.Y - e.ViewportDelta.Y) < 1 && e.OffsetDelta.Y == 0)
        {
            // Content length changed
        }
        else if (e.OffsetDelta.Y == 0)
        {
            // Length changing is happening at the top
            // Make the view watched by user remain
            messageScrollViewer.Offset = messageScrollViewer
                .Offset.WithY(
                    messageScrollViewer.Offset.Y +
                    e.ExtentDelta.Y
                );
            Console.WriteLine("Size changing is happening at the top");
        }


        // Avoid sending duplicate requests
        // Check if the number of messages changed
        if (_lastListMessagesLength == vm.ListMessages.Count)
        {
            // Not changed
            return;
        }


        // Check if the vertical offset is close to the top
        if (messageScrollViewer.Offset.Y > 150) return;


        // Load more messages
        if (vm.ListMessages.Count != 0 && vm.ShouldLoadMoreMessages)
        {
            vm.ShouldLoadMoreMessages = false;
            return;
        }

        _lastListMessagesLength = vm.ListMessages.Count;

        vm.LoadMoreMessagesCommand.Execute().Subscribe();
    }


    private void MessagesScrollViewer_OnInitialized(object? sender, EventArgs e)
    {
        if (sender is not ScrollViewer messageScrollViewer)
        {
            return;
        }

        if (DataContext is not ChatViewModel vm)
        {
            return;
        }


        vm.ReceiveChatMessageEvent += (_, _) =>
        {
            var distanceToBottom = messageScrollViewer.ScrollBarMaximum.Y - messageScrollViewer.Offset.Y;
            // Close to bottom and have new message
            if (distanceToBottom < 5)
            {
                // Scroll to new message
                messageScrollViewer.ScrollToEnd();
            }
        };
    }
}