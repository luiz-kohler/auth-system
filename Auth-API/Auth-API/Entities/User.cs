namespace Auth_API.Entities
{
    public class User 
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public virtual int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual IReadOnlyCollection<RoleUser> RolesUser { get; set; }
    }
}
