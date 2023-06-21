using System.Diagnostics;
using System.Net.Sockets;
using TcpChatLibrary.Models;
using TcpChatLibrary.Request;
using TcpChatLibrary.Tcp;

namespace TcpChatLibrary.Client;

public sealed class TcpServerClient{
    private static readonly object PadLock = new();

    private static TcpServerClient? _instance;

    private TcpClient? _tcpClient;
    
    private NetworkStream? _networkStream;

    private CancellationTokenSource? _cancellationTokenSource;

    public delegate void ResponseHandler(string response);

    public event ResponseHandler? ResponseReceived;

    private TcpServerClient(){ }

    public static TcpServerClient Instance{
        get{
            lock (PadLock){
                _instance ??= new TcpServerClient();
            }

            return _instance;
        }
    }

    public async Task ConnectAsync(string iPAddress, int port, string login, string password){
        try{
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());

            _tcpClient = new TcpClient();

            await _tcpClient.ConnectAsync(iPAddress, port);
            _networkStream = _tcpClient.GetStream();

            var authorizationData = new Request.Request
                    { Type = RequestType.Get, Body = new UserServerData{ Login = login, Password = password }.ToJson() }
                .ToJson();

            await TcpCommunicationHandler.SendMessageAsync(_networkStream, authorizationData);

            _ = TcpCommunicationHandler.ReceiveDataAsync(_tcpClient, _cancellationTokenSource.Token, (client, response) => {
                OnResponseReceived(response);
                return Task.CompletedTask;
            }, TcpCommunicationHandler.Disconnect);

            Debug.WriteLine("Connected");
        }
        catch (Exception exception){
            TcpCommunicationHandler.Disconnect(_tcpClient!);
            Debug.WriteLine(exception.Message);
        }
    }

    public async Task SendMessageAsync(string message){
        if (_networkStream != null) await TcpCommunicationHandler.SendMessageAsync(_networkStream, message);
    }

    public void Disconnect(){
        TcpCommunicationHandler.Disconnect(_tcpClient!);
    }

    private void OnResponseReceived(string response){
        ResponseReceived?.Invoke(response);
    }
}