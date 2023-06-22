using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TcpChatLibrary.Models;

[Table("Message")]
public class Message : Model, ICopyable{
    private string _messageText = string.Empty;

    private bool _isDelivered = false;

    public int SenderId{ get; set; }

    [JsonIgnore] [ForeignKey("SenderId")] public User? Sender{ get; set; }

    public int RecipientId{ get; set; }

    [JsonIgnore]
    [ForeignKey("RecipientId")]
    public User? Recipient{ get; set; }

    public string MessageText{
        get => _messageText;
        set{
            _messageText = value;
            OnPropertyChanged();
        }
    }

    public DateTime DispatchDateTime{ get; set; }

    public bool IsDelivered{
        get => _isDelivered;
        set{
            _isDelivered = value;
            OnPropertyChanged();
        }
    }

    public void CopyFrom(object other){
        if (other is not Message message) return;

        this.MessageText = message.MessageText;
        this.DispatchDateTime = message.DispatchDateTime;
        this.IsDelivered = message.IsDelivered;
    }

    public override string ToString(){
        return
            $"Message from: {Sender?.Login}, To: {Recipient?.Login}, Text: {MessageText}, Sent at: {DispatchDateTime}, Delivered: {IsDelivered}";
    }
}