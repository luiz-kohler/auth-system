namespace Auth_API.Entities
{
    public class RoleEndpoint
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public int ActionId { get; set; }
        public virtual Action Action { get; set; }
    }
}
