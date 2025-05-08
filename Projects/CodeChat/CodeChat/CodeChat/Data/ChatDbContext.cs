
using Microsoft.EntityFrameworkCore;
using CodeChat.Client.Components.Models;



namespace CodeChat.Data
{

    public class ChatDbContext : DbContext {

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Room> ChatRooms { get; set; }

    }
}




