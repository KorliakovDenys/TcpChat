namespace TcpChatLibrary.DbModels;

public class User{
    public int Id{ get; set; }

    public string Login{ get; set; } = null!;
}

public class UserServerData : User{
    public string Password{ get; set; } = null!;
}