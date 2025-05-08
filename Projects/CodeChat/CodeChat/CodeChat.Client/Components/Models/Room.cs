

namespace CodeChat.Client.Components.Models
{
    public class Room
    {
        public int RoomID { get; set; }                  //how do we want these to be generated?
        private string RoomKey { get; set; }
        private string RoomOwner { get; set; }
        public List<string> UserList { get; set; } = new List<string>();

        public Room()
        {
        }

        public Room(int roomID, string roomKey, string roomOwner, List<string> userList)
        {
            RoomID = roomID;
            RoomKey = roomKey;
            RoomOwner = roomOwner;
            UserList = userList;
        }

        public void AddUser(User requestingUser, User userToAdd)
        {
            if (this.RoomOwner != requestingUser.Username)
            { 
                throw new UnauthorizedAccessException($"Only {this.RoomOwner} can add users to the room.");
            }
            else if (!this.UserList.Contains(userToAdd.Username))
            {
                this.UserList.Add(userToAdd.Username);
                return;
            }
            else
            {
                throw new ArgumentException("User already exists in room. ");
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
