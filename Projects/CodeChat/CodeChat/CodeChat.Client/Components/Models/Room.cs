

namespace CodeChat.Client.Components.Models
{
    public class Room : 
    {
        public int RoomID { get; set; }                  //how do we want these to be generated?
        private string RoomKey { get; set; }            //how are these generated?
        private User RoomOwner { get; set; }
        public List<string> UserList { get; set; } = new List<string>();

        public Room(int roomID, string roomKey, User roomOwner, List<string> userList)
        {
            RoomID = roomID;
            RoomKey = roomKey;
            RoomOwner = roomOwner;
            UserList = userList;
        }

        public void AddUser(User requestingUser, User userToAdd)         //how do you make this method exclusive to the RoomOwner
        {
            if (this.RoomOwner != requestingUser)
            {
                //$"Only {this.RoomOwner.UserName} can add users to the room.");
                return;

            }
            if (!this.UserList.Contains(userToAdd.Username))
            {
                this.UserList.Add(userToAdd.Username);
                return;
            }
            else
            {
                //error message - populate where? 
            }
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
