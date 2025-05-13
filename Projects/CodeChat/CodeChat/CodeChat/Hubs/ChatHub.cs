/* * @file ChatHub.cs
 * 
 * @description: This file contains the ChatHub class which is used to handle chat messages and user authentication.
 * @author: CodeChat Team
 * @date: 2025-05-09
 * @version 0.2
 */
using CodeChat.Data;
using Microsoft.AspNetCore.SignalR;
using CodeChat.Client.Components.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CodeChat.Services;
using CodeChat.Services.Encryption;

namespace CodeChat.Hubs;

/* @class ChatHub
 * 
 * @description: This class is used to handle chat messages and user authentication.
 */
public class ChatHub : Hub
{
    private readonly ChatDbContext _db;
    private RoomService _roomService = new RoomService(new RoomEncryptionService());

    public ChatHub(ChatDbContext db) {
        _db = db;
    }

    /* @method SendMessage
     * 
     * @description: This method is used to send a message to all users in the chat room.
     * @param user: the username of the user sending the message
     * @param roomID: the ID of the chat room
     * @param message: the message to be sent
     * @param date: the date and time of the message
     */
    //public async Task DistributeEncryptedMessage(string roomID, ChatMessage message) {
    //    var room = _db.ChatRooms.FirstOrDefault(r => r.RoomID == roomID);
    //    if (room == null) {
    //        throw new HubException($"Room '{roomID}' not found");
    //    }
    //    AddEncryptedMessageToChatHistory(roomID, message);
    //    // get room userlist without the sender
    //    var userList = room.UserList.Where(u => u != message.Sender).ToList();
    //    // send the message to all users in the room
    //    foreach (var user in userList) {
    //        await Clients.User(user).SendAsync("ReceiveMessage", message);
    //    }
    //}

    public void AddEncryptedMessageToChatHistory(string roomID, ChatMessage message) {
        try {
            var room = _db.ChatRooms.FirstOrDefault(r => r.RoomID == roomID);
            if (room != null) {
                room.ChatHistory.Add(message);
                _db.SaveChanges();
                Clients.All.SendAsync("ChatHistoryUpdated", roomID, message);
            } else {
                throw new HubException($"Room '{roomID}' not found");
            }
        } catch (Exception ex) {
            throw new HubException($"Error adding message to chat history: {ex.Message}");
        }
    }

    /* @method CreateUser
     * 
     * @description: creates a new user in the database
     * @param username: the username of the user
     * @param password: the password of the user
     * @param email: the email of the user
     * @remarks: Key Pair is generated in the user class
     */
    public async Task<bool> CreateUser(string username, string password, string verifyPassword, string email, byte[] publicKey) {
        // attempt to add the user to the database
 
    
         try {
             if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
             {
                 await Clients.Caller.SendAsync("AccountCreationFailed", "Username, password, and email are required.");
                 return false;
             }
             else if (username.Length < 3 || username.Length > 20 || !username.All(char.IsLetterOrDigit))
             {
                 await Clients.Caller.SendAsync("AccountCreationFailed", "Username must be between 3 and 20 characters and contain only letters and digits.");
                 return false;
             
             }
             else if (await _db.Users.AnyAsync(u => u.Username == username))
             {
                 await Clients.Caller.SendAsync("AccountCreationFailed", $"Username '{username}' is already taken.");
                 return false;
             }
             else if (await _db.Users.AnyAsync(u => u.Email == email))
             {
                 await Clients.Caller.SendAsync("AccountCreationFailed", $"Email '{email}' already has a registered account.");
                 return false;
             }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
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
            else if (password != verifyPassword)
            {
                await Clients.Caller.SendAsync("AccountCreationFailed", "Passwords do not match");
                return false;
            }
            else
            {
            _db.Users.Add(new User { Username = username, Password = password, Email = email, PublicKey = publicKey });
            await _db.SaveChangesAsync();
            // user created successfully
            await Clients.Caller.SendAsync("AccountCreationSuccess", "Account created successfully! Redirecting to Homepage for login...");
            return true;
             }
         } 
        catch (Exception ex) {
             await Clients.Caller.SendAsync("AccountCreationFailed", $"Error creating user: {ex.Message}");
             return false; // handle the exception
         }
    }


    /* @method CheckUsername
     * 
     * @description: checks if a username already exists in the database
     * @param username: the username to check
     */
    public async Task<bool> CheckUsername(string username) {
        var userExists = await _db.Users.AnyAsync(u => u.Username == username);
        return userExists;
    }

    /* @method GetUsers
     * 
     * @description: gets all users from the database
     */
    public async Task<List<User>> GetUser() {
        var users = await _db.Users.ToListAsync();
        return users;
    }

    public async Task<List<string>> SearchUserDatabase(string username) {
        var users = await _db.Users.Where(u => u.Username.Contains(username)).Select(u => u.Username).ToListAsync();
        return users;
    }

    public async Task AddUserToRoom(byte[] roomKey, string username) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.UserList.Add(username);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("UserAddedToRoom", roomKey, username);
        }
    }

    public async Task<string> SearchRoomOwner(string roomID) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomID == roomID);
        if (room != null) {
            return room.RoomOwner;
        } else {
            throw new HubException($"Room with ID '{roomID}' not found");
        }
    }

    public async Task RemoveUserFromRoom(byte[] roomKey, string username) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.UserList.Remove(username);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("UserRemovedFromRoom", roomKey, username);
        }
    }

    public async Task<List<ChatMessage>> RequestChatHistory(string roomName) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
        if (room != null) {
            return room.ChatHistory;
        } else {
            throw new HubException($"Room with key '{roomName}' not found");
        }
    }

    public async Task ClearChatHistory(byte[] roomKey) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            room.ChatHistory.Clear();
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("ChatHistoryCleared", roomKey);
        }
    }

    public async Task<string> ValidateRoom(string roomName) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
        if (room != null) {
            return room.RoomID;
        } else {
            throw new HubException($"Room with ID '{roomName}' not found");
        }
    }

    /* @method CreateRoom
     * 
     * @description: creates a new chat room in the database, adds the users to the room,
     *  and sends the encrypted room key to the users
     * @param roomOwner: the username of the user creating the room
     * @param userList: the list of users to be added to the room
     */
    public async Task<bool> CreateRoom(string roomOwner, string roomName, List<string> userList) {
        
        var userKeyList = new List<string>();
        var roomID = Guid.NewGuid().ToString();
        var roomKey = _roomService.EncryptionService.GenerateRoomKey();

        // create the room and add it to the database
        var room = new Room { RoomID = roomID, RoomName = roomName,  RoomKey = roomKey, RoomOwner = roomOwner, UserList = userList };
        _db.ChatRooms.Add(room);
        await _db.SaveChangesAsync(); // save the changes
        // add the room to the user's list of chat rooms
        //var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == roomOwner);
        //if (user == null) {
        //    throw new HubException($"User '{roomOwner}' not found");
        //}
        //user.ChatRooms.Add(room.RoomName);
        //await _db.SaveChangesAsync(); // save the changes

        // return the result
        return await _db.ChatRooms.AnyAsync(r => r.RoomID == roomID);
    }

    public async Task<byte[]> RequestRoomKey(string user, string roomName) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
        if (room != null) {
            var userPublicKey = await _db.Users
                .Where(u => u.Username == user)
                .Select(u => u.PublicKey)
                .FirstOrDefaultAsync();
            if (userPublicKey == null) {
                throw new HubException($"User '{user}' not found");
            }
            var encryptedRoomKey = _roomService.EncryptPublicRoomKey(room.RoomKey, userPublicKey);
            return encryptedRoomKey;
        }
        throw new HubException($"Room '{roomName}' not found");
    }

    /* @method GetUserPublicKey
     * 
     * @description: gets the public key of a user from the database
     * @param username: the username of the user
     */
    public async Task<byte[]> GetUserPublicKey(string username) {
        var userPublicKey = await _db.Users
        .Where(u => u.Username == username)
        .Select(u => u.PublicKey)
        .FirstOrDefaultAsync();
        if (userPublicKey == null) {
            throw new HubException($"User '{username}' not found");
        }
        return userPublicKey;
    }

    public async Task<List<string>> GetOwnedChatRooms(string username) {
        var ownedRooms = await _db.ChatRooms
            .Where(r => r.RoomOwner == username)
            .Select(r => r.RoomName)
            .ToListAsync();
        return ownedRooms;
    }

    public async Task<List<string>> GetChatRooms(string username) {
        var chatRooms = await _db.ChatRooms
            .Where(r => r.UserList.Contains(username))
            .Select(r => r.RoomName)
            .ToListAsync();
        return chatRooms;
    }


    public async Task DeleteRoom(byte[] roomKey) {
        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.RoomKey == roomKey);
        if (room != null) {
            _db.ChatRooms.Remove(room);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("RoomDeleted", roomKey);
        }
    }

    /* @method AuthenticateUser
     * 
     * @description: authenticates a user by checking the username and password
     * @param username: the username of the user
     * @param password: the password of the user
     */
    public async Task<bool> AuthenticateUser(string username, string password) {

        var passwordHash = await _db.Users.Where(u => u.Username == username).Select(u => u.Password).FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await Clients.Caller.SendAsync("AccountAuthFailed", "Please enter both a username and password.");
            return false;
        }
        
        // Verifies that a passwordHash was created, i.e a username exists with a stored hasedpassword in the DB
        // -> need to add an explicit check for username?
        if (passwordHash == null)
        {
            await Clients.Caller.SendAsync("AccountAuthFailed", "Invalid username or password.");
            return false;
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(null, passwordHash, password) == PasswordVerificationResult.Success;

        if(result)
        {
            await Clients.Caller.SendAsync("AccountAuthSuccess", "Success: User authenticated.");
            return true;
        }
        else
        {
            await Clients.Caller.SendAsync("AccountAuthFailed", "Invalid username or password.");
            return false;
        }
    }
}



