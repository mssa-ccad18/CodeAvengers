using System.ComponentModel.DataAnnotations;

namespace CodeChat.Client.Components.Models
{
    public class ChatMessage
    {
        [Key] public int Id { get; set; }
        public string Sender { get; set; } = string.Empty; // sender of the message
        public byte[] Content { get; set; } = Array.Empty<byte>(); // content of the message
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // timestamp of the message
        public ChatMessage() { } // default constructor
        public ChatMessage(string sender, byte[] content) {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }
        public override string ToString() {
            return $"{Sender}: {Content} - {Timestamp}";
        }
    }
}
