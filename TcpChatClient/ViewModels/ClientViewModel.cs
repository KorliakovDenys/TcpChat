using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using TcpChatLibrary.Client;
using TcpChatLibrary.Json;
using TcpChatLibrary.Models;
using TcpChatLibrary.Request;

namespace TcpChatClient.ViewModels;

public sealed class ClientViewModel : ViewModel{
    private const string IpPattern =
        @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    private const string PortPattern = @"^(\d{1,5})$";

    private readonly HashSet<User> _uniqueUserCheck = new();

    private readonly HashSet<User> _uniqueContactCheck = new();

    private ObservableCollection<User> _users = new();

    private ObservableCollection<User> _contacts = new();

    private string _ip = string.Empty;

    private string _port = string.Empty;

    private string _messageInput = string.Empty;

    private DelegateCommand? _connectDelegateCommand;

    private DelegateCommand? _disconnectDelegateCommand;

    private DelegateCommand? _sendMessageDelegateCommand;

    private User _selectedUser = new ();

    private int _myId = -1;

    public ObservableCollection<User> Users{
        get => _users;
        set{
            _users = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<User> Contacts{
        get => _contacts;
        set{
            _contacts = value;
            OnPropertyChanged();
        }
    }

    public User SelectedUser{
        get => _selectedUser;
        set{
            _selectedUser = value;
            OnPropertyChanged();
        }
    }

    public string Ip{
        get => _ip;
        set{
            _ip = value;
            OnPropertyChanged();
        }
    }

    public string Port{
        get => _port;
        set{
            _port = value;
            OnPropertyChanged();
        }
    }

    public string MessageInput{
        get => _messageInput;
        set{
            _messageInput = value;
            OnPropertyChanged();
        }
    }

    public DelegateCommand ConnectDelegateCommand =>
        _connectDelegateCommand ??= new DelegateCommand(ExecuteConnect);

    public DelegateCommand DisconnectDelegateCommand =>
        _disconnectDelegateCommand ??= new DelegateCommand(ExecuteDisconnect);


    public DelegateCommand SendMessageDelegateCommand =>
        _sendMessageDelegateCommand ??= new DelegateCommand(ExecuteSendMessage);


    private void ExecuteConnect(){
        var isValidIpAddress = Regex.IsMatch(Ip, IpPattern);

        var isValidPort = Regex.IsMatch(Port, PortPattern);

        if (!isValidIpAddress){
            MessageBox.Show("Incorrect Ip Address.");
            return;
        }

        if (!isValidPort){
            MessageBox.Show("Incorrect Port.");
            return;
        }

        var authViewModel = new AuthorizationViewModel();

        var authWindow = new AuthorizationWindow(authViewModel);

        authViewModel.LogInPressed += () => {
            Connect(Ip, int.Parse(Port), authViewModel.Login, authViewModel.Password);
            authWindow.Close();
        };

        authViewModel.CancelPressed += () => { authWindow.Close(); };

        authWindow.ShowDialog();
    }


    private void Connect(string ip, int port, string login, string password){
        TcpServerClient.Instance.ResponseReceived += OnResponseReceived;
        _ = TcpServerClient.Instance.ConnectAsync(ip, port, login, password);
    }

    private void OnResponseReceived(string response){
        _ = HandleResponse(response);
    }

    private async Task HandleResponse(string response){
        try{
            var obj = JsonConvertor.ToObject(response);

            if (obj is not Request request) return;

            switch (JsonConvertor.ToObject(request.Body?.ToString() ?? string.Empty)){
                case UserServerData userServerData:{
                    _myId = userServerData.Id;
                    break;
                }
                case User userClientData:{
                    await HandleUser(request.Type, userClientData);
                    break;
                }
                case Message message:{
                    await HandleMessage(request.Type, message);
                    break;
                }
                case Contact contact:{
                    await HandleContact(request.Type, contact);
                    break;
                }
                default:
                    break;
            }


            Debug.WriteLine(response);
        }
        catch (Exception e){
            Console.WriteLine(e);
        }
    }

    private void ExecuteDisconnect(){
        Disconnect();
        ClearAll();
    }

    private void ExecuteSendMessage(){
        var message = new Message{
            RecipientId = SelectedUser.Id, SenderId = _myId, MessageText = MessageInput,
            DispatchDateTime = DateTime.Now
        };
        var request = new Request{
            Type = RequestType.Post,
            Body = message.ToJson()
        };

        HandleMessage(request.Type, message);

        _ = TcpServerClient.Instance.SendMessageAsync(request.ToJson());
        MessageInput = "";
    }

    private void AddUniqueUser(User user){
        if (_uniqueUserCheck.Add(user)){
            Users.Add(user);
        }
    }

    private void RemoveUniqueUser(User user){
        if (_uniqueUserCheck.Remove(user)){
            Users.Remove(user);
        }
    }

    private void AddUniqueContact(User user){
        if (_uniqueContactCheck.Add(user)){
            Contacts.Add(user);
        }
    }

    private void RemoveUniqueContact(User user){
        if (_uniqueContactCheck.Remove(user)){
            Contacts.Remove(user);
        }
    }

    private Task HandleUser(RequestType type, User user){
        try{
            switch (type){
                case RequestType.Post:{
                    AddUniqueUser(user);
                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    lock (new object()){
                        var userIndex = Users.IndexOf(user);
                        if (userIndex >= 0) Users[userIndex].CopyFrom(user);
                    }

                    break;
                }
                case RequestType.Delete:{
                    RemoveUniqueUser(user);

                    break;
                }
                default:
                    break;
            }
        }
        catch (Exception e){
            Debug.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    private Task HandleMessage(RequestType type, Message message){
        try{
            var sender = Users.First(u => u.Id == message.SenderId);

            var recipient = Users.First(u => u.Id == message.RecipientId);

            message.Sender = sender;

            message.Recipient = recipient;

            switch (type){
                case RequestType.Post:{
                    if (message.RecipientId == _myId && !message.IsDelivered){
                        message.IsDelivered = true;
                        var request = new Request{ Type = RequestType.Put, Body = message.ToJson() }.ToJson();
                        _ = TcpServerClient.Instance.SendMessageAsync(request);
                    }

                    if (sender.Id != _myId) sender.Messages?.Add(message);
                    recipient.Messages?.Add(message);
 
                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    var msg = sender.Messages?.First(m => m.Id == message.Id);
                    msg?.CopyFrom(message);
                    break;
                }
                case RequestType.Delete:{
                    sender.Messages?.Remove(message);

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

    private Task HandleContact(RequestType type, Contact contact){
        try{
            var contactUser = Users.First(u => u.Id == contact.ContactUserId);

            switch (type){
                case RequestType.Post:{
                    AddUniqueContact(contactUser);

                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    var first = contactUser.Contacts?.First(c => c.Id == contact.Id);

                    first?.CopyFrom(contact);

                    break;
                }
                case RequestType.Delete:{
                    RemoveUniqueContact(contactUser);

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

    private void Disconnect(){
        try{
            TcpServerClient.Instance.Disconnect();
            TcpServerClient.Instance.ResponseReceived -= OnResponseReceived;
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private void ClearAll(){
        Users.Clear();
        Contacts.Clear();
        _uniqueContactCheck.Clear();
        _uniqueUserCheck.Clear();
    }
}