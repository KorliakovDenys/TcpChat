namespace TcpChatLibrary.Models;

public class Message{
    public int Id{ get; set; }
    
    public int SenderId{ get; set; }
    
    public int RecipientId{ get; set; }
    
    public string MessageText{ get; set; } = null!;

    public DateTime DispatchDateTime{ get; set; }
    
    public bool IsDelivered{ get; set; }
}