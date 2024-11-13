using Auth_API.Common;

namespace Auth_API.Entities
{
    public class User : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RefreshTokenId { get; set; }
        public virtual RefreshToken? RefreshToken { get; set; }
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }
        public bool? IsUserOrganizationAdmin { get; set; }
        public virtual IReadOnlyCollection<RoleUser> RoleUsers { get; set; }
        public virtual IReadOnlyCollection<UserProject> UserProjects { get; set; }
    }
}
