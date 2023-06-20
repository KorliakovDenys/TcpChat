﻿using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TcpChatLibrary.Models;

[Table("Contact")]
public class Contact : Model{
    public int ContactOwnerId{ get; set; }

    [JsonIgnore]
    [ForeignKey("ContactOwnerId")]
    public User? ContactOwner{ get; set; }

    public int ContactUserId{ get; set; }

    [JsonIgnore]
    [ForeignKey("ContactUserId")]
    public User? ContactUser{ get; set; }

    public bool IsBlocked{ get; set; }

    public override string ToString(){
        return $"Contact: [ContactOwnerId={ContactOwnerId}, ContactOwner={ContactOwner!.Login}, " +
               $"ContactUserId={ContactUserId}, ContactUser={ContactUser!.Login}, IsBlocked={IsBlocked}]";
    }
}