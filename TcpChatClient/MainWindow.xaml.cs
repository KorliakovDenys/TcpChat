using System.Windows;
using TcpChatClient.ViewModels;

namespace TcpChatClient{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{
        private readonly ClientViewModel _viewModel = new();
        public MainWindow(){
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}