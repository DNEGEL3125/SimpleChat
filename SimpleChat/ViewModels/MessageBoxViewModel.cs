using System;
using System.Windows.Input;

namespace SimpleChat.ViewModels;

public class MessageBoxViewModel : ViewModelBase
{
    public string Text { get; set; } = string.Empty;
    public ICommand CloseCommand { get; set; }

    public MessageBoxViewModel()
    {
        throw new NotImplementedException();
    }
}