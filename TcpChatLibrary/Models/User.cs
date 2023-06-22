using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace TcpChatLibrary.Models;

[Table("User")]
public class User : Model, ICopyable{
    private string _login = string.Empty;

    private bool _isOnline = false;

    public string Login{
        get => _login;
        set{
            _login = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsOnline{
        get => _isOnline;
        set{
            _isOnline = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore] public ICollection<Message>? Messages{ get; set; } = new ObservableCollection<Message>();

    [JsonIgnore] public ICollection<Contact>? Contacts{ get; set; } = new ObservableCollection<Contact>();


    public User(){ }

    public User(User other){
        CopyFrom(other);
    }

    public void CopyFrom(object other){
        if(other is not User user) return;
        
        this.Id = user.Id;
        this.Login = user.Login;
        this.IsOnline = user.IsOnline;
    }

    public override string ToString(){
        var sb = new StringBuilder();
        sb.AppendLine($"Login: {Login}");
        sb.AppendLine($"IsOnline: {IsOnline}");
        sb.AppendLine("Messages:");

        foreach (var message in Messages!){
            sb.AppendLine($"- {message}");
        }

        sb.AppendLine("Contacts:");
        foreach (var contact in Contacts!){
            sb.AppendLine($"- {contact}");
        }

        return sb.ToString();
    }
}

[Table("UserServerData")]
public class UserServerData : User{
    public string Password{ get; set; } = null!;
}