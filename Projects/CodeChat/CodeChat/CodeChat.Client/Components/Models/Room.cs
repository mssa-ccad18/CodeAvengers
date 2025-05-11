

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CodeChat.Client.Components.Models
{
    public class Room
    {
       
        [Key]
        public string RoomID { get; set; }                  //how do we want these to be generated?


        [Required]
        public string RoomName { get; set; } = string.Empty; // name of the room
        public byte[] RoomKey { get; set; }
        public string RoomOwner { get; set; }
        public List<string> UserList { get; set; } = new List<string>();
        public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>(); // list of chat messages in the room

        public Room() {}

        public Room(byte[] roomKey, string roomOwner, string roomName, List<string> userList)
        {
            RoomKey = roomKey;
            RoomOwner = roomOwner;
            RoomName = roomName;
            UserList = userList;
            userList.Add(roomOwner); //add the room owner to the list of users
        }

        public void AddUser(string requestingUser, string userToAdd)
        {
        }

        public void RemoveUser(User userToRemove)
        {
            if (!this.UserList.Contains(userToRemove.Username))
            {
                //error user not in room
                return;
            }
            else this.UserList.Remove(userToRemove.Username); //how would removal from the list of approved users boot the user from the room?

        }

        public void ClearChatHistory()
        {
            //clear the chat message history via button on chat GUI
        }

        //Delete chatroom function?



    }
}
