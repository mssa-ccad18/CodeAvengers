using Microsoft.JSInterop;
﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeChat.Client.Components.Models
{
    public class User
    {
        IPasswordHasher<User> hasher = new PasswordHasher<User>();
        private string hashedPassword = string.Empty;

        public User() { } // default constructor

        // Key property for EF Core to map the User entity to a database
        // table with a unique identifier for each row, enabling operations like EnsureCreated() to succeed.
        [Key]
        public int Id { get; set; }

        // required properties for the User entity
        [Required]
        public required string Username { get; set; } 
        public required string Email { get; set; } = string.Empty;
        public required string Password  {
            get { return hashedPassword; }
            set
            {
                if (!string.IsNullOrEmpty(value) && hashedPassword == string.Empty) {
                    hashedPassword = hasher.HashPassword(null!, value);
                }
            }
        }
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();  // public key for the user

        // optional properties for the User entity
        [NotMapped]
        public List<Room> ChatRooms { get; set; } = new List<Room>();  //List of chatrooms a member of by RoomID (int) | should this be a dictionary? roomname + roomID

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
        => (hasher.VerifyHashedPassword(this, Password, password) == PasswordVerificationResult.Success);

        override
        public string ToString() {
            return $"Username: {Username}";
        }
    }
}
