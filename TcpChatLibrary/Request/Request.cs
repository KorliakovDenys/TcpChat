using TcpChatLibrary.Models;

namespace TcpChatLibrary.Request;

public class Request: Model{
    public RequestType Type{ get; set; }
    
    public object? Body{ get; set; }
}