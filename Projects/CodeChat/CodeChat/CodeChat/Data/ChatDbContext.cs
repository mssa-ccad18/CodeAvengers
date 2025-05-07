using CodeChat.Client.Components.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace CodeChat.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
