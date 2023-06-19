﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TcpChatClient.ViewModels;

public abstract class ViewModel : INotifyPropertyChanged{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null){
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}