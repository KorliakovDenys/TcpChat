using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace TcpChatLibrary.Models;

[Table("User")]
public class User : Model{
    public string Login{ get; set; } = null!;

    public bool IsOnline{ get; set; }

    [JsonIgnore]
    public ICollection<Message>? Messages{ get; set; } = new List<Message>();

    [JsonIgnore] 
    public ICollection<Contact>? Contacts{ get; set; } = new List<Contact>();


    public User(){ }

    public User(User other){
        Id = other.Id;
        Login = other.Login;
        IsOnline = other.IsOnline;
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