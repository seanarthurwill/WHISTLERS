namespace Whistl3rApi.Models.DTOs
{
    public class UserNameDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
