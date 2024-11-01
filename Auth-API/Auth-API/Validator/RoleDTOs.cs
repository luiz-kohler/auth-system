using Auth_API.Common;

namespace Auth_API.Validator
{
    public class CreateRoleRequest
    {
        public string Name { get; set; }
        public int ProjectId { get; set; }
    }

    public class GetManyRolesRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? ProjectId { get; set; }
        public int? EndpointId { get; set; }
        public int? UserId { get; set; }
    }

    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProjectForRoleResponse Project { get; set; }
    }

    public class ProjectForRoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoleWithRelationsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProjectForRoleResponse Project { get; set; }
        public IEnumerable<EndpointForRoleResponse> Endpoints { get; set; }
        public IEnumerable<UserForRoleResponse> Users { get; set; }
    }

    public class UserForRoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class EndpointForRoleResponse
    {
        public int Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }
}
