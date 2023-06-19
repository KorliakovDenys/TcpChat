using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TcpChatLibrary.Models;

namespace TcpChatLibrary.Client;

public class TcpServerClient{
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
            _tcpClient = new TcpClient();

            await _tcpClient.ConnectAsync(iPAddress, port);
            _networkStream = _tcpClient.GetStream();

            var authorizationData =
                JsonConvert.SerializeObject(new UserServerData{ Login = login, Password = password });

            await SendMessageAsync(authorizationData);

            _ = ReceiveMessagesAsync();

            Debug.WriteLine("Connected");
        }
        catch (Exception exception){
            Disconnect();
            Debug.WriteLine(exception.Message);
        }
    }

    public void Disconnect(){
        try{
            _networkStream?.Close();
            _tcpClient?.Close();
            Debug.WriteLine("Disconnect() called");
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private async Task ReceiveMessagesAsync(){
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());

        try{
            var buffer = new byte[1024];
            var messageBuilder = new StringBuilder();

            while (!_cancellationTokenSource!.Token.IsCancellationRequested){
                var bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length - 1);

                if (bytesRead == 0){
                    break;
                }

                var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(receivedData);

                var receivedMessage = messageBuilder.ToString();

                var message = receivedMessage.Trim();
                
                OnResponseReceived(message);

                messageBuilder.Clear();
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
        finally{
            Disconnect();
            Debug.WriteLine("Receiving stopped");
        }
    }

    private async Task SendMessageAsync(string message){
        Debug.WriteLine(message + " sent");

        var data = Encoding.UTF8.GetBytes(message);
        await _networkStream!.WriteAsync(data, 0, data.Length);
        await _networkStream.FlushAsync().ConfigureAwait(false);
    }

    protected virtual void OnResponseReceived(string response){
        ResponseReceived?.Invoke(response);
    }
}