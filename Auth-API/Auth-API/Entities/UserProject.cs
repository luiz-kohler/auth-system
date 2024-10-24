using Auth_API.Common;

namespace Auth_API.Entities
{
    public class UserProject : IBaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}
