using CodeChat.Data;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeChat.Client.Components.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CodeChat.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _db;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ChatDbContext db, ILogger<ChatHub> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SendMessage(string user, string message, string date)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message, date);
    }

    public async Task CreateUser(string username, string publicKey)
    {
        try
        {
            _logger.LogInformation("CreateUser method called with username: {Username}", username);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(publicKey))
            {
                _logger.LogWarning("Invalid input - Username or PublicKey is empty");
                throw new HubException("Username and Public Key are required");
            }

            _logger.LogInformation("Checking for existing user with username: {Username}", username);
            if (await _db.Users.AnyAsync(u => u.Username == username))
            {
                _logger.LogWarning("Username already exists: {Username}", username);
                throw new HubException($"Username '{username}' is already taken");
            }

            _logger.LogInformation("Creating new user: {Username}", username);
            var user = new User { Username = username, PublicKey = publicKey };
            _db.Users.Add(user);
            
            _logger.LogInformation("Saving changes to database");
            var result = await _db.SaveChangesAsync();
            _logger.LogInformation("Database save completed. Rows affected: {RowsAffected}", result);

            _logger.LogInformation("Notifying clients about new user");
            await Clients.All.SendAsync("UserCreated", user);
            _logger.LogInformation("User creation process completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateUser method for username: {Username}", username);
            throw new HubException($"Error creating user: {ex.Message}");
        }
    }
}

