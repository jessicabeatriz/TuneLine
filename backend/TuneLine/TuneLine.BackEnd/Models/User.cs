namespace TuneLine.BackEnd.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime ExpirationDate { get; set; }

    }
}
