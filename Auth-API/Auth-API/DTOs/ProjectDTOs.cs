namespace Auth_API.DTOs
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public int ManagerId { get; set; }
        public List<string> Endpoints { get; set; }
    }
}
