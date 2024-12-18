﻿namespace Auth_API.DTOs
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class DeleteUserRequest
    {
        public int Id { get; set; }
    }

    public class VerifyUserHasAccessRequest
    {
        public int EndpointId { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GetUserTokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
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

    public class VerifyUserHasAccessResponse
    {
        public bool HasAccess { get; set; }
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
