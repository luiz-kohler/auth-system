using Auth_API.Common;

namespace Auth_API.Entities
{
    public class Project : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IReadOnlyCollection<Role> Roles { get; set; }
        public virtual IReadOnlyCollection<Endpoint> Endpoints { get; set; }
        public virtual IReadOnlyCollection<UserProject> UserProjects { get; set; }
    }
}
