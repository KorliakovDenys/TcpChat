using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TcpChatLibrary;

public static class JsonConvertor {
    public static string ToJson(object obj){
        var settings = new JsonSerializerSettings{
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            ContractResolver = new DefaultContractResolver{
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        
        var jsonString = JsonConvert.SerializeObject(obj, settings);

        return jsonString;
    }
}