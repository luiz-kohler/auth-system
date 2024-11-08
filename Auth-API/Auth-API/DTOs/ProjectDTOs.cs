using Auth_API.Common;

namespace Auth_API.DTOs
{
    public class GetOneProjectRequest
    {
        public int Id { get; set; }
    }

    public class DeleteOneProjectRequest
    {
        public int Id { get; set; }
    }


    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public List<EndpointForProjectRequests> Endpoints { get; set; }
    }

    public class EndpointForProjectRequests
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

    public class UpsertProjectRequest : CreateProjectRequest
    {
    }
}
