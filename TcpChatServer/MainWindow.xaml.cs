
using System.Windows;
using TcpChatLibrary.Server;

namespace TcpChatServer{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{
        private readonly ServerViewModel _serverViewModel = new();
        public MainWindow(){
            InitializeComponent();
            DataContext = _serverViewModel;
        }
    }
}