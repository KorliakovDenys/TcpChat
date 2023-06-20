using System.Text.RegularExpressions;
using System.Windows;
using TcpChatLibrary.Models;
using Prism.Commands;
using TcpChatLibrary.Server;


namespace TcpChatServer;

public class ServerViewModel : ViewModel{
    private const string PortPattern = @"^(\d{1,5})$";

    private string _port = string.Empty;
    
    private DelegateCommand? _startDelegateCommand;

    private DelegateCommand? _stopDelegateCommand;

    public DelegateCommand StartDelegateCommand => _startDelegateCommand ??= new DelegateCommand(ExecuteStart);

    public DelegateCommand StopDelegateCommand => _stopDelegateCommand ??= new DelegateCommand(ExecuteStop);
    
    public string Port{
        get => _port;
        set{
            _port = value;
            OnPropertyChanged();
        }
    }
    
    private void ExecuteStart(){
        var isValidPort = Regex.IsMatch(Port, PortPattern);

        if (!isValidPort){
            MessageBox.Show("Incorrect Port.");
            return;
        }
        
        _ = TcpServer.Instance.StartAsync(int.Parse(Port));
    }
    private void ExecuteStop(){
        TcpServer.Instance.Stop();
    }
}