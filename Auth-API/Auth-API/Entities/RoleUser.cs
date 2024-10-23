using Auth_API.Common;

namespace Auth_API.Entities
{
    public class RoleUser : IBaseEntity
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
