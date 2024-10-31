using Auth_API.Common;

namespace Auth_API.DTOs
{
    public class CreateEndpointRequest
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    public class GetManyEndpointRequest
    {
        public int? Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod? HttpMethod { get; set; }
        public bool? IsPublic { get; set; }
        public int? ProjectId { get; set; }
        public int? RoleId { get; set; }
    }

    public class EndpointResponse
    {
        public int Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
        public IEnumerable<RoleToEndpointResponse> Roles { get; set; }
        public ProjectToEndpointResponse Project { get; set; }
    }

    public class RoleToEndpointResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProjectToEndpointResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
