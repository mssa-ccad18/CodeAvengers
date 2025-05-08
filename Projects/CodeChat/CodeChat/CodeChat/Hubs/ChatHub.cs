
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

    public ChatHub(ChatDbContext db) {
        _db = db;
    }

    public async Task SendMessage(string user, string message, string date) {
        await Clients.All.SendAsync("ReceiveMessage", user, message, date);
    }

    public async Task CreateUser(string username, string password, string email) {

        try {

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {
                throw new HubException("Username and Public Key are required");
            }

            if (await _db.Users.AnyAsync(u => u.Username == username)) {
                throw new HubException($"Username '{username}' is already taken");
            }

            var user = new User { Username = username, Password = password, Email = email };
            _db.Users.Add(user);


            var result = await _db.SaveChangesAsync();



            await Clients.All.SendAsync("UserCreated", user);
        } catch (Exception ex) {
            throw new HubException($"Error creating user: {ex.Message}");
        }

    }

    public async Task<List<User>> GetUsers() {
        var users = await _db.Users.ToListAsync();
        return users;
    }

    public async Task<List<Room>> GetChatRooms() {
        var chatrooms = await _db.ChatRooms.ToListAsync();
        return chatrooms;
    }

    public async Task AddUserToRoom(string roomKey, string username) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.UserList.Add(username);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("UserAddedToRoom", roomKey, username);
        }
    }

    public async Task RemoveUserFromRoom(string roomKey, string username) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.UserList.Remove(username);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("UserRemovedFromRoom", roomKey, username);
        }
    }

    public async Task ClearChatHistory(string roomKey) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.UserList.Clear();
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("ChatHistoryCleared", roomKey);
        }
    }

    public async Task CreateRoom(string roomKey, string roomOwner, List<string> userList) {
        var room = new Room { RoomKey = roomKey, RoomOwner = roomOwner, UserList = userList };
        _db.ChatRooms.Add(room);
        await _db.SaveChangesAsync();
        await Clients.All.SendAsync("RoomCreated", room);
    }

    public async Task DeleteRoom(string roomKey) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            _db.ChatRooms.Remove(room);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("RoomDeleted", roomKey);
        }
    }

    public async Task<bool> AuthenticateUser(string username, string password) {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user != null && user.VerifyPassword(password);
    }
}

