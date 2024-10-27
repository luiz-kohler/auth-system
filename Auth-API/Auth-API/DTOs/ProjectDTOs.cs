using Auth_API.Common;

namespace Auth_API.DTOs
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public int ManagerId { get; set; }
        public List<EndpointForCreateProject> Endpoints { get; set; }
    }

    public class EndpointForCreateProject
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }
}
