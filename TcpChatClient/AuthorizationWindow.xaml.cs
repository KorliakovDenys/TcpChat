using System.Windows;
using System.Windows.Controls;
using TcpChatClient.ViewModels;

namespace TcpChatClient;

public partial class AuthorizationWindow : Window{
    private readonly AuthorizationViewModel _authorizationViewModel;
    
    public AuthorizationWindow(AuthorizationViewModel viewModel){
        InitializeComponent();
        _authorizationViewModel = viewModel;
        DataContext = _authorizationViewModel;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e){
        if (sender is not PasswordBox passwordBox) return;

        _authorizationViewModel.Password = passwordBox.Password;
    }
}