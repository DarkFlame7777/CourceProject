namespace MyProject.Models.Entities
{
    public class EmailToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public TokenType Type { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;

        public User User { get; set; } = null!;
    }
}
