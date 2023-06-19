using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using Prism.Commands;
using TcpChatClient.Client;
using TcpChatLibrary.Models;

namespace TcpChatClient;

public sealed class ClientViewModel : INotifyPropertyChanged{
    private const string IpPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    private const string PortPattern = @"^(\d{1,5})$";

    private string _ip = string.Empty;
    
    private string _port = string.Empty;

    private DelegateCommand? _connectDelegateCommand;
   
    private DelegateCommand? _disconnectDelegateCommand;

    public ObservableCollection<User> OnlineUsers{ get; } = new ();

    public ObservableCollection<Contact> Contacts{ get; } = new ();

    public DelegateCommand ConnectDelegateCommand => _connectDelegateCommand ??= new DelegateCommand(ExecuteConnect);
    
    public DelegateCommand DisconnectDelegateCommand => _disconnectDelegateCommand ??= new DelegateCommand(ExecuteDisconnect);

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
        
        
    }
    
    private void ExecuteDisconnect(){
        
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null){
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}