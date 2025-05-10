
﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static CodeChat.Client.Pages.Chat;

namespace CodeChat.Client.Components.Models
{
    public class User
    {
        IPasswordHasher<User> hasher = new PasswordHasher<User>();
        private string hashedPassword = string.Empty;

        [Key]
        public int Id { get; set; } //required property for EF Core to map the User entity to a database table with a unique identifier for each row, enabling operations like EnsureCreated() to succeed.

        [Required]
        public required string Username { get; set; } 
        public required string Email { get; set; } 
        public  string Password  {   //removed required
            get { return hashedPassword; }
            set
            {
                if (hashedPassword == string.Empty) this.hashedPassword = hasher.HashPassword(null, value);
            }
        }
        public string PublicKey { get; set; }
        public List<Room> ChatRooms { get; set; } = new List<Room>();  //List of chatrooms a member of by RoomID (int) | should this be a dictionary? roomname + roomID


        public User()  //do we need to define the constructor? 
        {
            PublicKey = string.Empty;
        }

        public void JoinRoom(Room room)
        {
            if (!ChatRooms.Contains(room))
            {
                ChatRooms.Add(room);
            }
        }

        public void LeaveRoom(Room room)
        {
            ChatRooms.Remove(room);
        }

        public bool VerifyPassword(string password)
        {
            var hasher = new PasswordHasher<User>();
            return hasher.VerifyHashedPassword(null, this.Password, password) == PasswordVerificationResult.Success;

        }

        override
        public string ToString() {
            return $"Username: {Username}";
        }


    }
}
