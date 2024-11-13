namespace Auth_API.Entities
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IReadOnlyCollection<User> Users { get; set; }
        public virtual IReadOnlyCollection<Project> Projects { get; set; }
    }
}
