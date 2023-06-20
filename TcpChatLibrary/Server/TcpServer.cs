using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using TcpChatLibrary.Models;
using TcpChatLibrary.Data;
using TcpChatLibrary.Json;
using TcpChatLibrary.Request;
using static TcpChatLibrary.Tcp.TcpCommunicationHandler;

namespace TcpChatLibrary.Server;

public sealed class TcpServer{
    private static readonly object PadLock = new();

    private static TcpServer? _instance;

    private readonly TcpChatDataContext _dataContext =
        new("Server=127.0.0.1,1433;Database=TcpChat;User Id=Server;Password=qwe123;TrustServerCertificate=True");

    private readonly ConcurrentDictionary<UserServerData, List<TcpClient>> _authorizedTcpClients = new();

    private TcpListener? _tcpListener;

    private CancellationTokenSource? _cancellationTokenSource;

    private TcpServer(){ }

    public static TcpServer Instance{
        get{
            lock (PadLock){
                _instance ??= new TcpServer();
            }

            return _instance;
        }
    }

    public async Task StartAsync(int port){
        _cancellationTokenSource = new CancellationTokenSource();

        try{
            var localAddress = IPAddress.Parse("127.0.0.1");

            _tcpListener = new TcpListener(localAddress, port);

            _tcpListener.Start();
            Debug.WriteLine("Started");

            while (!_cancellationTokenSource.Token.IsCancellationRequested){
                var client = await _tcpListener.AcceptTcpClientAsync();

                Debug.WriteLine("Connected " + client.Client.RemoteEndPoint);

                _ = ReceiveDataAsync(client, _cancellationTokenSource.Token, HandleResponseAsync, DisconnectClient);
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

    

    private async Task HandleResponseAsync(TcpClient sender, string response){
        try{
            var obj = JsonConvertor.ToObject(response);

            if (obj is not Request.Request request) return;

            switch (JsonConvertor.ToObject(request.Body?.ToString() ?? string.Empty)){
                case User user:{
                    await HandleUser(sender, request.Type, user);
                    break;
                }
                case Message message:{
                    await HandleMessage(sender, request.Type, message);
                    break;
                }
                case Contact contact:{
                    await HandleContact(sender, request.Type, contact);
                    break;
                }
                default:
                    break;
            }

            await _dataContext.SaveChangesAsync();

            Debug.WriteLine(response);
        }
        catch (Exception e){
            Console.WriteLine(e);
        }
    }

    private void DisconnectClient(TcpClient tcpClient){
        foreach (var userServerData in _authorizedTcpClients.Keys){
            foreach (var client in _authorizedTcpClients[userServerData].Where(client => client.Equals(tcpClient))){
                _authorizedTcpClients[userServerData].Remove(client);
                if (_authorizedTcpClients[userServerData].Count == 0){
                    userServerData.IsOnline = false;
                }
            }
        }
    }

    private async Task HandleUser(TcpClient sender, RequestType type, User user){
        var senderStream = sender.GetStream();
        try{
            switch (type){
                case RequestType.Post:{
                    break;
                }
                case RequestType.Get:{
                    if (user is not UserServerData userServerData){ return; }

                    var fetchedUserServerData = _dataContext.UsersServerData!.First(u => u.Login == userServerData.Login);
                    
                    if (fetchedUserServerData.Password != userServerData.Password) return;

                    fetchedUserServerData.IsOnline = true;

                    userServerData = fetchedUserServerData;
                    
                    var tcpClientsList = _authorizedTcpClients.GetOrAdd(userServerData, _ => new List<TcpClient>());

                    tcpClientsList.Add(sender);
                        
                    await foreach (var u in _dataContext.Users!){
                        await SendMessageAsync(senderStream,
                            new Request.Request{ Type = RequestType.Post, Body = u?.ToJson() }.ToJson());
                    }

                    await foreach (var c in _dataContext.Contacts!){
                        if (c.ContactOwnerId == userServerData?.Id){
                            await SendMessageAsync(senderStream,
                                new Request.Request{ Type = RequestType.Post, Body = c?.ToJson() }.ToJson());
                        }
                    }

                    await foreach (var c in _dataContext.Messages!){
                        if (c.RecipientId == userServerData?.Id || c.SenderId == userServerData?.Id){
                            await SendMessageAsync(senderStream,
                                new Request.Request{ Type = RequestType.Post, Body = c?.ToJson() }.ToJson());
                        }
                    }

                    break;
                }
                case RequestType.Put:{
                    break;
                }
                case RequestType.Delete:{
                    break;
                }
                default:
                    break;
            }
        }
        catch (Exception e){
            senderStream.Close();
            Debug.WriteLine(e);
        }
    }

    private async Task HandleMessage(TcpClient sender, RequestType type, Message message){
        try{
            var recipientContact = _dataContext.Contacts!.First(c =>
                c.ContactOwnerId == message.RecipientId && c.ContactUserId == message.SenderId);
            
            if (recipientContact.IsBlocked){
                return;
            }
            
            switch (type){
                case RequestType.Post:{
                    var userServerData = _authorizedTcpClients.Keys.First(u => u.Id == message.RecipientId);

                    foreach (var tcpClient in _authorizedTcpClients[userServerData]){
                        await foreach (var msg in _dataContext.Messages!){
                            await SendMessageAsync(tcpClient.GetStream(),
                                new Request.Request{ Type = RequestType.Post, Body = msg.ToJson() }.ToJson());
                        }
                    }

                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    break;
                }
                case RequestType.Delete:{
                    break;
                }
                default:
                    break;
            }
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private Task HandleContact(TcpClient sender, RequestType type, Contact contact){
        try{
            switch (type){
                case RequestType.Post:{
                    _dataContext.Contacts?.Add(contact);
                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    _dataContext.Contacts?.Remove(contact);
                    _dataContext.Contacts?.Add(contact);
                    break;
                }
                case RequestType.Delete:{
                    _dataContext.Contacts?.Remove(contact);
                    break;
                }
                default:
                    break;
            }
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    
}