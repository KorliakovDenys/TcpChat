using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpChatLibrary.Models;
using TcpChatServer.Data;

namespace TcpChatServer.Server;

public class TcpServer{
    private const int Port = 9000;

    private static readonly object PadLock = new();

    private static TcpServer? _instance;
    
    private readonly TcpChatDataContext _dataContext = new ();

    private readonly ConcurrentDictionary<UserServerData, TcpClient> _clients = new();

    private TcpListener? _tcpListener;

    private CancellationTokenSource? _cancellationTokenSource;
    
    private TcpServer(){ }

    public static TcpServer Instance{
        get{
            lock (PadLock){
                _instance = new TcpServer();
            }

            return _instance;
        }
    }

    public async Task Start(){
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());

        try{
            var localAddress = IPAddress.Parse("127.0.0.1");

            _tcpListener = new TcpListener(localAddress, Port);

            _tcpListener.Start();

            while (!_cancellationTokenSource.Token.IsCancellationRequested){
                var client = await _tcpListener.AcceptTcpClientAsync();

                _ = HandleClient(client);
            }
        }
        catch (Exception exception){
            Debug.WriteLine(exception.Message);
        }
        finally{
            _tcpListener?.Stop();
        }
    }
    
    public void Stop(){
        _cancellationTokenSource?.Cancel();
    }
    
    private async Task HandleClient(TcpClient client){
        
    }

   
}