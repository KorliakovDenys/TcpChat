using System;
using Prism.Commands;
using TcpChatLibrary.Models;

namespace TcpChatClient.ViewModels;

public class AuthorizationViewModel : ViewModel{
    private string _login = string.Empty;

    private string _password = string.Empty;

    private DelegateCommand? _logInDelegateCommand;

    private DelegateCommand? _cancelDelegateCommand;

    public DelegateCommand LogInDelegateCommand => _logInDelegateCommand ??= new DelegateCommand(ExecuteLogIn);

    public DelegateCommand CancelDelegateCommand => _cancelDelegateCommand ??= new DelegateCommand(ExecuteCancel);

    public event Action? LogInPressed;
    
    public event Action? CancelPressed;

    public string Login{
        get => _login;
        set{
            _login = value;
            OnPropertyChanged();
        }
    }

    public string Password{
        get => _password;
        set{
            _password = value;
            OnPropertyChanged();
        }
    }

    private void ExecuteLogIn(){
        LogInPressed?.Invoke();
    }
    private void ExecuteCancel(){
        CancelPressed?.Invoke();
    }

}