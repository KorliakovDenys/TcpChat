﻿namespace TcpChatLibrary.Models;

public class User{
    public int Id{ get; set; }

    public string Name{ get; set; } = null!;
}

public class UserServerData : User{
    public string Password{ get; set; } = null!;
}