
﻿using Microsoft.AspNetCore.Identity;
using static CodeChat.Client.Pages.Chat;

namespace CodeChat.Client.Components.Models
{
    public class User
    {
        IPasswordHasher<User> hasher = new PasswordHasher<User>();
        public string hashedPassword = string.Empty;

        public int Id { get; set; } //required property for EF Core to map the User entity to a database table with a unique identifier for each row, enabling operations like EnsureCreated() to succeed.

       
        public string Username { get; set; } //add required?

        public string Password //add required?
        {
            get { return hashedPassword; }
            set
            {
                if (hashedPassword == string.Empty) this.hashedPassword = hasher.HashPassword(null, value);
            }
        }
        

        public string PublicKey { get; set; }
        public List<int> ChatRooms { get; set; } = new List<int>();  //List of chatrooms a member of by RoomID (int) | should this be a dictionary? roomname + roomID

        //public User(string username, string password)  //do we need to define the constructor? No because being instantiated via the login page where properties/fields will be assigned 
        //{
        //    Username = username;
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


        public bool VerifyPassword(string password)
        => (hasher.VerifyHashedPassword(null, this.Password, password) == PasswordVerificationResult.Success)
            ? true : false;


    }
}
