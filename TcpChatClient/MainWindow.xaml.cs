using System.Windows;

namespace TcpChatClient{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{
        public MainWindow(){
            InitializeComponent();
        }

        private void ConnectButtonBase_OnClick(object sender, RoutedEventArgs e){
            _ = TcpChatClientController.Instance.ConnectAsync("127.0.0.1", "admin", "password").ConfigureAwait(false);
        }
        
        private void SendButtonBase_OnClick(object sender, RoutedEventArgs e){
            _ = TcpChatClientController.Instance.SendMessageAsync("message123");
        }
        
        private void DisconnectButtonBase_OnClick(object sender, RoutedEventArgs e){
            TcpChatClientController.Instance.Disconnect();
        }
    }
}