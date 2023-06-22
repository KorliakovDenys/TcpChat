using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TcpChatLibrary.Json;

namespace TcpChatLibrary.Models;

public abstract class Model : ViewModel, IJsonAble {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id{ get; set; }

    public string ToJson(){
        return JsonConvertor.ToJson(this);
    }

    public override bool Equals(object? obj){
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (Model)obj;
        return Equals(other);
    }

    private bool Equals(Model other){
        return Id == other.Id;
    }

    public override int GetHashCode(){
        return Id.GetHashCode();
    }
}