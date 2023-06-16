using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpChatClient.Client;

public class TcpChatClient{
    private const int Port = 9000;

    private static readonly object PadLock = new();

    private static TcpChatClient? _instance;

    private TcpClient? _tcpClient;

    private NetworkStream? _networkStream;
    
    private CancellationTokenSource? _cancellationTokenSource;


    private TcpChatClient(){ }

    public static TcpChatClient Instance{
        get{
            lock (PadLock){
                _instance = new TcpChatClient();
            }

            return _instance;
        }
    }

    public async Task Connect(string iPAddress){
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());
        try{
            _tcpClient = new TcpClient();

            await _tcpClient.ConnectAsync(iPAddress, Port);
            _networkStream = _tcpClient.GetStream();

            _ = ReceiveMessages();

            while (!_cancellationTokenSource.Token.IsCancellationRequested){ }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
    }

    private async Task ReceiveMessages(){
        try{
            var buffer = new byte[1024];
            var messageBuilder = new StringBuilder();

            while (!_cancellationTokenSource!.Token.IsCancellationRequested){
                
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
    }
}