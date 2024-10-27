namespace Auth_API.DTOs
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GetUserTokenResposne
    {
        public string Token { get; set; }
    }

    public class GetManyUsersRequest
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? ProjectId { get; set; }
        public int? RoleId { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<ProjectForUserResponse> Projects { get; set; }
        public IEnumerable<RoleForUserResponse> Roles { get; set; }
    }

    public class ProjectForUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoleForUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
