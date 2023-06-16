namespace TcpChatLibrary.Models;

public class Contact{
     public int Id{ get; set; }
     
     public int ContactOwnerId{ get; set; }
     
     public int ContactUserId{ get; set; }
     
     public bool IsBlocked{ get; set; }
}