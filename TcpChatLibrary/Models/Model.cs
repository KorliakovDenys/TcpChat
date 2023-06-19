namespace TcpChatLibrary;

public abstract class Model : IJsonAble{
    public string ToJson(){
        return JsonConvertor.ToJson(this);
    }
}