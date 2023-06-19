
using System.Windows;

namespace TcpChatServer{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{
        public MainWindow(){
            InitializeComponent();
        }

        private void StartButtonBase_OnClick(object sender, RoutedEventArgs e){
            _ = Server.TcpServer.Instance.StartAsync();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e){
            Server.TcpServer.Instance.Stop();
        }
    }
}