using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpChatLibrary.Models;
using TcpChatLibrary.Data;

namespace TcpChatLibrary.Server;

public class TcpServer{
    
    private const int Port = 9000;

    private static readonly object PadLock = new();

    private static TcpServer? _instance;
    
    private readonly TcpChatDataContext _dataContext = new ();

    private readonly ConcurrentDictionary<UserServerData, List<TcpClient>> _authorizedTcpClients = new();

    private TcpListener? _tcpListener;

    private CancellationTokenSource? _cancellationTokenSource;
    
    private TcpServer(){ }

    public static TcpServer Instance{
        get{
            lock (PadLock){
                _instance??= new TcpServer();
            }

            return _instance;
        }
    }

    public async Task StartAsync(){
        _cancellationTokenSource = new CancellationTokenSource();

        try{
            var localAddress = IPAddress.Parse("127.0.0.1");

            _tcpListener = new TcpListener(localAddress, Port);

            _tcpListener.Start();
            Debug.WriteLine("Started");
            
            while (!_cancellationTokenSource.Token.IsCancellationRequested){
                var client = await _tcpListener.AcceptTcpClientAsync();

                Debug.WriteLine("Connected " + client.Client.RemoteEndPoint);
                
                _ = HandleClientAsync(client);
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
        finally{
            _tcpListener?.Stop();
            Debug.WriteLine("Stopped");
        }
    }
    
    public void Stop(){
        _cancellationTokenSource?.Cancel();
        _tcpListener?.Stop();
    }

    private async Task HandleClientAsync(TcpClient client){
        var networkStream = client.GetStream();
        
        try{
            var buffer = new byte[1024];
            var messageBuilder = new StringBuilder();

            while (!_cancellationTokenSource!.Token.IsCancellationRequested){
                var bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length-1).ConfigureAwait(false);

                if (bytesRead == 0){
                    break;
                }

                var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(receivedData);

                var receivedMessage = messageBuilder.ToString();

                var message = receivedMessage.Trim();

                _ = HandleMessageAsync(client, message).ConfigureAwait(false);

                messageBuilder.Clear();
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
        finally{
            Debug.WriteLine(client.Client.RemoteEndPoint + " disconnected");
            networkStream.Close();
            client.Close();
        }
    }

    private async Task HandleMessageAsync(TcpClient sender, string message){
        Debug.WriteLine(message);

        await SendMessageAsync(sender.GetStream(), message).ConfigureAwait(false);
    }
    
    public async Task SendMessageAsync(NetworkStream networkStream, string message){
        var data = Encoding.UTF8.GetBytes(message);
        await networkStream.WriteAsync(data, 0, data.Length);
        await networkStream.FlushAsync().ConfigureAwait(false);
        
        Debug.WriteLine(message + " sent");
    }
}