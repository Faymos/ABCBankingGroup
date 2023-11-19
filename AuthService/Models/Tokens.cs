namespace AuthService.Models
{
    public class Tokens
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; }
    }
}
