using Microsoft.EntityFrameworkCore;
using TcpChatLibrary.Models;

namespace TcpChatServer.Data;

public class TcpChatDataContext : DbContext{
    public DbSet<UserServerData>? Users{ get; set; }
    
    public DbSet<Contact>? Contacts{ get; set; }

    public DbSet<Message>? Messages{ get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlServer(
            "Server=127.0.0.1,1433;Database=TcpChat;User Id=Server;Password=qwe123;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Message>()
            .Property(c => c.IsDelivered)
            .HasColumnType("bit");
        
        modelBuilder.Entity<Contact>()
            .Property(c => c.IsBlocked)
            .HasColumnType("bit");
    }
}