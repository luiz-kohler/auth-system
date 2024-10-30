using Auth_API.Common;

namespace Auth_API.DTOs
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public int AdminId { get; set; }
        public List<EndpointForCreateProject> Endpoints { get; set; }
    }

    public class EndpointForCreateProject
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    public class ProjectToGetManyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetProjectByIdResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<UserToGetProjectByIdResponse> Users { get; set; }
        public IEnumerable<EndpointToGetProjectByIdResponse> Endpoints { get; set; }
        public IEnumerable<RoleToGetProjectByIdResponse> Roles { get; set; }
    }

    public class UserToGetProjectByIdResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class EndpointToGetProjectByIdResponse
    {
        public int Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    public class RoleToGetProjectByIdResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
