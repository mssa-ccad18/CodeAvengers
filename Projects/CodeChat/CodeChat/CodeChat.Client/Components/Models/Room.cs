using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CodeChat.Client.Components.Models
{
    public class Room
    {
        [Key]
        [Required]
        public required string RoomID { get; set; }

        [Required]
        public required string RoomName { get; set; } = string.Empty;
        
        [Required]
        public required byte[] RoomKey { get; set; }
        
        [Required]
        public required string RoomOwner { get; set; }
        
        public List<string> UserList { get; set; } = new List<string>();
        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();

        public Room() {}

        public Room(byte[] roomKey, string roomOwner, string roomName, List<string> userList)
        {
            RoomKey = roomKey;
            RoomOwner = roomOwner;
            RoomName = roomName;
            UserList = userList;
            userList.Add(roomOwner);
        }

        public void AddUser(string requestingUser, string userToAdd)
        {
        }

        public void RemoveUser(User userToRemove)
        {
            if (!this.UserList.Contains(userToRemove.Username))
            {
                return;
            }
            else this.UserList.Remove(userToRemove.Username);
        }

        public void ClearChatHistory()
        {
            ChatHistory.Clear();
        }

        //Delete chatroom function?
    }
}
