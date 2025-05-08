

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CodeChat.Client.Components.Models
{
    public class Room
    {
        // get the injected db
        HttpClient HttpClient;

        [Key]
        public int RoomID { get; set; }                  //how do we want these to be generated?


        [Required]
        public string RoomKey { get; set; }
        public string RoomOwner { get; set; }
        public List<string> UserList { get; set; } = new List<string>();

        public Room() {}

        public Room(string roomKey, string roomOwner, List<string> userList, HttpClient http)
        {
            RoomKey = roomKey;
            RoomOwner = roomOwner;
            UserList = userList;
            HttpClient = http;
        }

        public void AddUser(string requestingUser, string userToAdd)
        {
            var users = HttpClient.GetAsync("ChatHub/GetUsers").Result;
            // parse the json result
            var userList = JsonSerializer.Deserialize<List<User>>(users.Content.ReadAsStringAsync().Result);
            // check if the user exists
            if (userList == null || !userList.Any(u => u.Username == userToAdd) || userList.Find(u => u.Username == requestingUser) == null) {
                //error message - populate where? 
                return;
            }
            else if (RoomOwner != requestingUser)
            { 
                throw new UnauthorizedAccessException($"Only {RoomOwner} can add users to the room.");
            }
            else if (!UserList.Contains(userToAdd))
            {
                UserList.Add(userToAdd);
                return;
            }
        }

        public void RemoveUser(User userToRemove)
        {
            if (!this.UserList.Contains(userToRemove.Username))
            {
                throw new ArgumentException("That username is not associated with this room.");
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
