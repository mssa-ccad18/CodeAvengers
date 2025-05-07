
﻿using CodeChat.Data;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeChat.Client.Components.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


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

        try
        {
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(publicKey))
            {
                throw new HubException("Username and Public Key are required");
            }

            if (await _db.Users.AnyAsync(u => u.Username == username))
            {
                throw new HubException($"Username '{username}' is already taken");
            }


            var user = new User { Username = username, PublicKey = publicKey };
            _db.Users.Add(user);
            

            var result = await _db.SaveChangesAsync();



            await Clients.All.SendAsync("UserCreated", user);
        }
        catch (Exception ex)
        {
            throw new HubException($"Error creating user: {ex.Message}");
        }

    }
}

