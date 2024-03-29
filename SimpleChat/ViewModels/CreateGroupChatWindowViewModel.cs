using System.Reactive;
using ReactiveUI;
using SimpleChat.Models;

namespace SimpleChat.ViewModels;

public class CreateGroupChatWindowViewModel : ViewModelBase
{
    private string _chatTitle = string.Empty;
    public ReactiveCommand<Unit, CreateChatForm> CreateGroupChatCommand { get; }


    public CreateGroupChatWindowViewModel()
    {
        CreateGroupChatCommand = ReactiveCommand.Create(
            () => new CreateChatForm
            {
                ChatTitle = _chatTitle
            }
        );
    }

    public string ChatTitle
    {
        get => _chatTitle;
        set => this.RaiseAndSetIfChanged(ref _chatTitle, value);
    }
}