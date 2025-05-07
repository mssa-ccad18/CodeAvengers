//using Microsoft.AspNetCore.Identity;
using static CodeChat.Client.Pages.Chat;

namespace CodeChat.Client.Components.Models
{
    public class User
    {
        //IPasswordHasher<User> hasher = new PasswordHasher<User>();
        public string hashedPassword = string.Empty;

        public string Username { get; set; }

        public string Password { get; set; }

        //public string Password
        //{
        //    get { return hashedPassword; }
        //    set
        //    {
        //        if (hashedPassword == string.Empty) this.hashedPassword = hasher.HashPassword(null, value);
        //    }
        //}


        public string PublicKey { get; set; }
        public List<int> ChatRooms { get; set; } = new List<int>();  //List of chatrooms a member of by RoomID (int) | should this be a dictionary? roomname + roomID

        //public User(string userName, string password)  //do we need to define the constructor? 
        //{
        //    UserName = userName;
        //    Password = password;
        //    //Key
        //}

        public void JoinRoom(Room room)
        {
            if (!ChatRooms.Contains(room.RoomID))
            {
                ChatRooms.Add(room.RoomID);
            }
        }

        public void LeaveRoom(Room room)
        {
            ChatRooms.Remove(room.RoomID);
        }


        //public bool VerifyPassword(string plainText)
        //=> (hasher.VerifyHashedPassword(null, this.Password, plainText) == PasswordVerificationResult.Success)
        //    ? true : false;


    }
}
