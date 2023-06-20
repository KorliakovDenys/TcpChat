using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TcpChatLibrary.Models;

[Table("Message")]
public class Message : Model{
    public int SenderId{ get; set; }
    
    [JsonIgnore] 
    [ForeignKey("SenderId")] 
    public User? Sender{ get; set; }

    public int RecipientId{ get; set; }

    [JsonIgnore]
    [ForeignKey("RecipientId")]
    public User? Recipient{ get; set; }

    public string MessageText{ get; set; } = null!;

    public DateTime DispatchDateTime{ get; set; }

    public bool IsDelivered{ get; set; }

    public override string ToString(){
        return
            $"Message from: {Sender?.Login}, To: {Recipient?.Login}, Text: {MessageText}, Sent at: {DispatchDateTime}, Delivered: {IsDelivered}";
    }
}