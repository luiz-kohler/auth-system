namespace Auth_API.DTOs
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public virtual int ProjectId { get; set; }
    }
}
