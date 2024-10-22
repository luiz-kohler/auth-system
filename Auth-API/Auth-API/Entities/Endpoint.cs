namespace Auth_API.Entities
{
    public class Endpoint
    {
        public int Id { get; set; }
        public int Route { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual IReadOnlyCollection<RoleEndpoint> RolesEndpoint { get; set; }
    }
}
