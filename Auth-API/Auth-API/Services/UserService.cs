﻿using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Handlers;
using Auth_API.Repositories;
using Auth_API.Validator;

namespace Auth_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserProjectRepository _userProjectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleUserRepository _roleUserRepository;
        private readonly ITokenHandler _tokenHandler;
        private readonly IHashHandler _hashHandler;
        private readonly IRefreshTokenHandler _refreshTokenHandler;

        public UserService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IUserProjectRepository userProjectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            ITokenHandler tokenHandler,
            IHashHandler hashHandler,
            IRefreshTokenHandler refreshTokenHandler)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _userProjectRepository = userProjectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _tokenHandler = tokenHandler;
            _hashHandler = hashHandler;
            _refreshTokenHandler = refreshTokenHandler;
        }

        public async Task<GetUserTokenResponse> Create(CreateUserRequest request)
        {
            var result = new CreateUserValidator().Validate(request);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            var isThereUserWithSameEmail = await _userRepository.Any(user => user.Email == request.Email);
            if (isThereUserWithSameEmail)
                throw new BadRequestException("There is already a registered user using this email");

            var user = new User();
            user.Name = request.Name;
            user.Email = request.Email;
            user.Password = _hashHandler.Hash(request.Password);

            await _userRepository.Add(user);
            await _userRepository.Commit();

            return await GenerateUserTokenResponse(user);
        }

        public async Task<GetUserTokenResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetSingle(user => user.Email == request.Email);

            if (user == null || !_hashHandler.Verify(request.Password, user.Password))
                throw new BadRequestException("Incorrect credentials");

            return await GenerateUserTokenResponse(user);
        }

        public async Task<GetUserTokenResponse> RefreshToken(RefreshTokenRequest request)
        {
            var token = await _refreshTokenHandler.Refresh(request.Token, request.RefreshToken);

            return new GetUserTokenResponse
            {
                Token = token,
                RefreshToken = request.RefreshToken
            };
        }

        private async Task<GetUserTokenResponse> GenerateUserTokenResponse(User user) => new()
        {
            Token = _tokenHandler.Generate(user),
            RefreshToken = await _refreshTokenHandler.Generate(user)
        };

        public async Task<IEnumerable<UserResponse>> GetMany(GetManyUsersRequest request)
        {
            var users = await _userRepository.GetAll(user =>
                (!request.Id.HasValue || user.Id == request.Id.Value) &&
                (string.IsNullOrEmpty(request.Name) || user.Name.Contains(request.Name)) &&
                (string.IsNullOrEmpty(request.Email) || user.Email == request.Email) &&
                (!request.ProjectId.HasValue || user.UserProjects.Select(userProject => userProject.ProjectId).Contains(request.ProjectId.Value)) &&
                (!request.RoleId.HasValue || user.RoleUsers.Select(userRole => userRole.RoleId).Contains(request.RoleId.Value)));

            return users.Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Projects = user.UserProjects.Select(userProject => userProject.Project).Select(project => new ProjectForUserResponse
                {
                    Id = project.Id,
                    Name = project.Name
                }),
                Roles = user.RoleUsers.Select(roleUser => roleUser.Role).Select(role => new RoleForUserResponse
                {
                    Id = role.Id,
                    Name = role.Name
                })
            });
        }

        public async Task<User> ExtractUserFromCurrentSession()
        {
            var userId = _tokenHandler.ExtractUserIdFromCurrentSession();

            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException();

            return user;
        }

        public async Task<VerifyUserHasAccessResponse> VerifyUserHasAccess(int endpointId)
        {
            var userId = _tokenHandler.ExtractUserIdFromCurrentSession();

            return new VerifyUserHasAccessResponse
            {
                HasAccess = await _userRepository.UserHasAccess(userId, endpointId)
            };
        }

        public async Task VerifyUserIsProjectAdmin(int projectId)
        {
            var user = await ExtractUserFromCurrentSession();

            if (!user.RoleUsers.Any(ru => ru.Role.ProjectId == projectId &&
                                          ru.Role.Name == EDefaultRole.Admin.GetDescription()))
                throw new UnauthorizedAccessException("User is not an admin for the specified project.");
        }

        public async Task VerifyUserIsOrganizationAdmin(int organizationId)
        {
            var user = await ExtractUserFromCurrentSession();

            if (!user.OrganizationId.HasValue)
                throw new UnauthorizedAccessException("User is not associated with any organization.");

            if (user.OrganizationId != organizationId)
                throw new UnauthorizedAccessException("User is not part of the specified organization.");

            if (user.IsUserOrganizationAdmin != true)
                throw new UnauthorizedAccessException("User is not an admin for the specified organization.");
        }
    }

    public interface IUserService
    {
        Task<GetUserTokenResponse> Create(CreateUserRequest request);
        Task<GetUserTokenResponse> Login(LoginRequest request);
        Task<GetUserTokenResponse> RefreshToken(RefreshTokenRequest request);
        Task<IEnumerable<UserResponse>> GetMany(GetManyUsersRequest request);
        Task<User> ExtractUserFromCurrentSession();
        Task<VerifyUserHasAccessResponse> VerifyUserHasAccess(int endpointId);
        Task VerifyUserIsProjectAdmin(int projectId);
        Task VerifyUserIsOrganizationAdmin(int organizationId);
    }
}
