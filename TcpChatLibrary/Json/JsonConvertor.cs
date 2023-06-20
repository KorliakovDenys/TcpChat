using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TcpChatLibrary.Json;

public static class JsonConvertor {
    private static readonly JsonSerializerSettings Settings = new (){
        TypeNameHandling = TypeNameHandling.All,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        ContractResolver = new DefaultContractResolver{
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };
    
    public static string ToJson(object obj){
        
        var jsonString = JsonConvert.SerializeObject(obj, Settings);

        return jsonString;
    }

    public static object? ToObject(string json){
        return JsonConvert.DeserializeObject(json, Settings);
    }
}