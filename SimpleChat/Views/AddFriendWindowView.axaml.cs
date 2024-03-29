using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleChat.ViewModels;

namespace SimpleChat.Views;

public partial class AddFriendWindowView : Window
{
    public AddFriendWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}