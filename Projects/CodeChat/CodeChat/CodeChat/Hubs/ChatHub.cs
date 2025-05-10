
﻿using CodeChat.Data;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CodeChat.Client.Components.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


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

    public async Task<bool> CreateUser(string username,string email, string password, string verifyPassword) {

        try {

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email)) {

                await Clients.Caller.SendAsync("AccountCreationFailed", "Username, password, and email are required.");
                return false;
            }

            if (username.Length < 3 || username.Length > 20 || !username.All(char.IsLetterOrDigit))
            {
                await Clients.Caller.SendAsync("AccountCreationFailed", "Username must be between 3 and 20 characters and contain only letters and digits.");
                return false;
            }

            if (await _db.Users.AnyAsync(u => u.Username == username)) {

                await Clients.Caller.SendAsync("AccountCreationFailed", $"Username '{username}' is already taken.");
                return false;
            }

            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                await Clients.Caller.SendAsync("AccountCreationFailed", $"Email '{email}' already has a registered account.");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Clients.Caller.SendAsync("AccountCreationFailed", "Invalid email format.");
                return false;
            }

            // Disabled for ease during testing 
            //if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            //{
            //    await Clients.Caller.SendAsync("AccountCreationFailed", "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");
            //    return false;
            //}

            if(password != verifyPassword)
            {
                await Clients.Caller.SendAsync("AccountCreationFailed", "Passwords do not match");
                return false;
            }

            var user = new User { Username = username, Password = password, Email = email };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await Clients.Caller.SendAsync("AccountCreationSuccess", "Account created successfully! Redirecting to Homepage for login...");
            return true;
            } 
        catch (Exception ex) {
            await Clients.Caller.SendAsync("AccountCreationFailed", $"Error creating user: {ex.Message}");
            return false;
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
        
        var passwordHash = await _db.Users.Where(u => u.Username == username).Select(u => u.Password).FirstOrDefaultAsync() ;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await Clients.Caller.SendAsync("AccountCreationFailed", "Please enter both a username and password.");
            return false;
        }

        // Verifies that a passwordHash was created, i.e a username exists with a stored hasedpassword in the DB
        // -> need to add an explicit check for username?
        if (passwordHash == null)
        {
            await Clients.Caller.SendAsync("AccountCreationFailed", "Invalid username or password.");
            return false;
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(null, passwordHash, password) == PasswordVerificationResult.Success;

        if(result)
        {
            await Clients.Caller.SendAsync("AccountCreationSuccess", "Success: User authenticated.");
            return true;
        }
        else
        {
            await Clients.Caller.SendAsync("AccountCreationFailed", "Invalid username or password.");
            return false;
        }




    }
}

