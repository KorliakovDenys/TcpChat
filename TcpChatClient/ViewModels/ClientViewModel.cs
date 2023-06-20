using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using TcpChatLibrary.Client;
using TcpChatLibrary.Data;
using TcpChatLibrary.Json;
using TcpChatLibrary.Models;
using TcpChatLibrary.Request;

namespace TcpChatClient.ViewModels;

public sealed class ClientViewModel : ViewModel{
    private const string IpPattern =
        @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    private const string PortPattern = @"^(\d{1,5})$";

    private readonly TcpChatDataContext _tcpChatDataContext = new();

    private string _ip = string.Empty;

    private string _port = string.Empty;

    private DelegateCommand? _connectDelegateCommand;

    private DelegateCommand? _disconnectDelegateCommand;

    public ObservableCollection<User?> OnlineUsers{ get; } = new();

    public ObservableCollection<Contact> Contacts{ get; } = new();

    public DelegateCommand ConnectDelegateCommand => _connectDelegateCommand ??= new DelegateCommand(ExecuteConnect);

    public DelegateCommand DisconnectDelegateCommand =>
        _disconnectDelegateCommand ??= new DelegateCommand(ExecuteDisconnect);


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
                case User user:{
                    await HandleUser(request.Type, user);
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

    private Task HandleUser(RequestType type, User? user){
        try{
            switch (type){
                case RequestType.Post:{
                    _tcpChatDataContext.Users?.Add(user);
                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    _tcpChatDataContext.Users?.Remove(user);
                    _tcpChatDataContext.Users?.Add(user);

                    break;
                }
                case RequestType.Delete:{
                    _tcpChatDataContext.Users?.Remove(user);

                    break;
                }
                default:
                    break;
            }

            _tcpChatDataContext.SaveChanges();
        }
        catch (Exception e){
            Debug.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    private Task HandleMessage(RequestType type, Message message){
        try{
            switch (type){
                case RequestType.Post:{
                    _tcpChatDataContext.Messages?.Add(message);

                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    _tcpChatDataContext.Messages?.Remove(message);
                    _tcpChatDataContext.Messages?.Add(message);

                    break;
                }
                case RequestType.Delete:{
                    _tcpChatDataContext.Messages?.Remove(message);

                    break;
                }
                default:
                    break;
            }

            _tcpChatDataContext.SaveChanges();
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    private Task HandleContact(RequestType type, Contact contact){
        try{
            switch (type){
                case RequestType.Post:{
                    _tcpChatDataContext.Contacts?.Add(contact);

                    break;
                }
                case RequestType.Get:{
                    break;
                }
                case RequestType.Put:{
                    _tcpChatDataContext.Contacts?.Remove(contact);
                    _tcpChatDataContext.Contacts?.Add(contact);

                    break;
                }
                case RequestType.Delete:{
                    _tcpChatDataContext.Contacts?.Remove(contact);

                    break;
                }
                default:
                    break;
            }

            _tcpChatDataContext.SaveChanges();
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    private void ExecuteDisconnect(){
        Disconnect();
        ClearAll();
    }

    private void Disconnect(){
        try{
            TcpServerClient.Instance.Disconnect();
            // TcpServerClient.Instance.ResponseReceived -= OnResponseReceived;
        }
        catch (Exception e){
            Debug.WriteLine(e);
        }
    }

    private void ClearAll(){
        OnlineUsers.Clear();
        Contacts.Clear();
    }
}