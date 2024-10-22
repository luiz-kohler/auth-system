namespace Auth_API.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual IReadOnlyCollection<RoleUser> RoleUsers { get; set; }
        public virtual IReadOnlyCollection<RoleEndpoint> RoleEndpoints { get; set; }
    }
}
