using Auth_API.Common;

namespace Auth_API.Entities
{
    public class RoleEndpoint : IBaseEntity
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public int EndpointId { get; set; }
        public virtual Endpoint Endpoint { get; set; }
    }
}
