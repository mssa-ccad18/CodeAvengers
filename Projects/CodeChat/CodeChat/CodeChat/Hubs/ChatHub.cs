using CodeChat.Client.Components.Models;
using CodeChat.Data;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;



namespace CodeChat.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _db;

    public ChatHub(ChatDbContext db)
    {
        _db = db;
    }

    public async Task SendMessage(string user, string message, string date)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message, date);
    }

    public async Task CreateUser(string username, string publicKey)
    {
        var user = new User { Username = username, PublicKey = publicKey };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await Clients.All.SendAsync("UserCreated", user);
    }
}
