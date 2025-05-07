namespace CodeChat.Client.Components.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PublicKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
