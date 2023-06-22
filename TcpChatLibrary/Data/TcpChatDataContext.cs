using Microsoft.EntityFrameworkCore;
using TcpChatLibrary.Models;

namespace TcpChatLibrary.Data;

public class TcpChatDataContext : DbContext{
    private readonly string _connectionString;

    public DbSet<User>? Users{ get; set; }
    
    public DbSet<UserServerData>? UsersServerData{ get; set; }

    public DbSet<Contact>? Contacts{ get; set; }

    public DbSet<Message>? Messages{ get; set; }

    public TcpChatDataContext(){
        _connectionString = "Server=127.0.0.1,1433;Database=TcpChat;User Id=Server;Password=qwe123;TrustServerCertificate=True";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Message>()
            .Property(m => m.IsDelivered)
            .HasColumnType("bit");

        modelBuilder.Entity<Contact>()
            .Property(c => c.IsBlocked)
            .HasColumnType("bit");

        modelBuilder.Entity<User>()
            .Property(u => u.IsOnline)
            .HasColumnType("bit");

        modelBuilder.Entity<User>().UseTptMappingStrategy();

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.ContactOwner)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.ContactOwnerId);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.ContactUser)
            .WithMany()
            .HasForeignKey(c => c.ContactUserId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId);
    }
}