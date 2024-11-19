using Auth_API.Common;
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

            return new()
            {
                Token = _tokenHandler.Generate(user),
                RefreshToken = await _refreshTokenHandler.Generate(user)
            };
        }

        public async Task<GetUserTokenResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetSingle(user => user.Email == request.Email);

            if (user == null || !_hashHandler.Verify(request.Password, user.Password))
                throw new BadRequestException("Incorrect credentials");

            return new()
            {
                Token = _tokenHandler.Generate(user),
                RefreshToken = await _refreshTokenHandler.Generate(user)
            };
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
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

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task Delete(int userId)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            if (user.UserProjects.Any() || user.RoleUsers.Any())
                throw new BadRequestException("You must remove this users from all roles and project");

            await _userRepository.Delete(user);
            await _userRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task AddToProjects(int userId, List<int> projectIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            projectIds = projectIds.Distinct().ToList();

            var linkedProjectIds = user.UserProjects
                .Select(userProject => userProject.ProjectId)
                .Intersect(projectIds)
                .Distinct()
                .ToList();

            if (linkedProjectIds.Any())
                throw new BadRequestException($"User is already linked with the projects: {string.Join(", ", linkedProjectIds)}");

            var projects = await _projectRepository.GetAll(project => projectIds.Contains(project.Id));

            if (projectIds.Count != projects.Count())
                throw new BadRequestException($"Some informed projects was not found");

            var newUserProjects = projects.Select(project => new UserProject { User = user, Project = project });

            await _userProjectRepository.Add(newUserProjects);
            await _userProjectRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task RemoveFromProjects(int userId, List<int> projectIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            var distinctProjectIds = projectIds.Distinct().ToList();

            var userProjectIds = user.UserProjects.Select(up => up.ProjectId).ToHashSet();
            if (distinctProjectIds.Any(id => !userProjectIds.Contains(id)))
                throw new BadRequestException("User is not linked in all informed projects");

            var userProjectsToRemove = user.UserProjects
                .Where(up => distinctProjectIds.Contains(up.ProjectId))
                .ToList();

            var projectsWithSingleAdmin = userProjectsToRemove
                .Where(up => up.Project.Roles
                    .Any(role => role.Name == EDefaultRole.Admin.GetDescription() &&
                                 role.RoleUsers.Count == 1 &&
                                 role.RoleUsers.Any(roleUser => roleUser.UserId == userId)))
                .ToList();

            if (projectsWithSingleAdmin.Any())
                throw new BadRequestException("User is the only admin in some projects and can not be removed");

            var userProjectsIdsToRemove = userProjectsToRemove.Select(userProject => userProject.ProjectId).ToList();

            await _userProjectRepository
                .DeleteWhere(userProject => userProject.UserId == userId
                                         && userProjectsIdsToRemove.Contains(userProject.ProjectId));

            await _roleUserRepository.DeleteWhere(roleUser => roleUser.UserId == userId && distinctProjectIds.Contains(roleUser.Role.ProjectId));

            await _userProjectRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task AddToRoles(int userId, List<int> roleIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            roleIds = roleIds.Distinct().ToList();

            var linkedRoleIds = user.RoleUsers
                .Select(roleUser => roleUser.RoleId)
                .Intersect(roleIds)
                .Distinct()
                .ToList();

            if (linkedRoleIds.Any())
                throw new BadRequestException($"User is already linked in the roles: {string.Join(", ", linkedRoleIds)}");

            var roles = await _roleRepository.GetAll(role => roleIds.Contains(role.Id));

            if (roleIds.Count != roles.Count())
                throw new BadRequestException($"Some informed roles was not found");

            var projectIdsUserIsLinked = user.UserProjects.Select(userProject => userProject.ProjectId).ToHashSet();
            var projectIdsOfEachRole = roles.Select(role => role.ProjectId).Distinct();

            var projectIdsUserNotLinked = projectIdsOfEachRole
                .Where(projectId => !projectIdsUserIsLinked.Contains(projectId))
                .ToList();

            if (projectIdsUserNotLinked.Any())
                throw new BadRequestException($"User must be linked in the projects: {string.Join(", ", projectIdsUserNotLinked)}");

            var newUserProjects = roles.Select(role => new RoleUser { User = user, Role = role });

            await _roleUserRepository.Add(newUserProjects);
            await _roleUserRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task RemoveFromRoles(int userId, List<int> roleIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            var distinctRoleIds = roleIds.Distinct().ToList();

            var userRoleIds = user.RoleUsers.Select(up => up.RoleId).ToHashSet();
            if (distinctRoleIds.Any(id => !userRoleIds.Contains(id)))
                throw new BadRequestException("User is not linked in all informed roles");

            var userRolesToRemove = user.RoleUsers
                .Where(up => distinctRoleIds.Contains(up.RoleId))
                .ToList();

            var rolesWithSingleAdmin = userRolesToRemove
                .Where(ru => ru.Role.Name == EDefaultRole.Admin.GetDescription() &&
                             ru.Role.RoleUsers.Count == 1 &&
                             ru.Role.RoleUsers.Any(roleUser => roleUser.UserId == userId))
                .ToList();

            if (rolesWithSingleAdmin.Any())
                throw new BadRequestException("User is the only admin in some projects and can not be removed");

            var userRolesIdsToRemove = userRolesToRemove.Select(userRole => userRole.RoleId).ToList();

            await _roleUserRepository
                .DeleteWhere(userRole => userRole.UserId == userId
                                         && userRolesIdsToRemove.Contains(userRole.RoleId));

            await _roleUserRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task<VerifyUserHasAccessResponse> VerifyUserHasAccess(int endpointId)
        {
            var userId = _tokenHandler.ExtractUserIdFromCurrentSession();

            return new VerifyUserHasAccessResponse
            {
                HasAccess = await _userRepository.UserHasAccess(userId, endpointId)
            };
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task<GetUserTokenResponse> RefreshToken(RefreshTokenRequest request)
        {
            var token = await _refreshTokenHandler.Refresh(request.Token, request.RefreshToken);

            return new GetUserTokenResponse
            {
                Token = token,
                RefreshToken = request.RefreshToken
            };
        }

        public async Task<User> ExtractUserFromCurrentSession()
        {
            var userId = _tokenHandler.ExtractUserIdFromCurrentSession();
            return await _userRepository.GetSingle(user => user.Id == userId);
        }

        public async Task VerifyUserIsProjectAdmin(int projectId)
        {
            var user = await ExtractOrganizationUserOrThrow();
            if (!user.RoleUsers.Any(ru => ru.Role.ProjectId == projectId &&
                                          ru.Role.Name == EDefaultRole.Admin.GetDescription()))
                throw new UnauthorizedAccessException("User is not an admin for the specified project.");
        }

        public async Task VerifyUserIsOrganizationAdmin(int organizationId)
        {
            var user = await ExtractOrganizationUserOrThrow();

            if (user.OrganizationId != organizationId)
                throw new UnauthorizedAccessException("User is not part of the specified organization.");

            if (user.IsUserOrganizationAdmin != true)
                throw new UnauthorizedAccessException("User is not an admin for the specified organization.");
        }

        private async Task<User> ExtractOrganizationUserOrThrow()
        {
            var user = await ExtractUserFromCurrentSession();
            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (!user.OrganizationId.HasValue)
                throw new UnauthorizedAccessException("User is not associated with any organization.");

            return user;
        }
    }

    public interface IUserService
    {
        Task<GetUserTokenResponse> Create(CreateUserRequest request);
        Task<GetUserTokenResponse> Login(LoginRequest request);
        Task<IEnumerable<UserResponse>> GetMany(GetManyUsersRequest request);
        Task Delete(int userId);
        Task AddToProjects(int userId, List<int> projectIds);
        Task RemoveFromProjects(int userId, List<int> projectIds);
        Task AddToRoles(int userId, List<int> roleIds);
        Task RemoveFromRoles(int userId, List<int> roleIds);
        Task<VerifyUserHasAccessResponse> VerifyUserHasAccess(int endpointId);
        Task<GetUserTokenResponse> RefreshToken(RefreshTokenRequest request);
        Task<User> ExtractUserFromCurrentSession();
        Task VerifyUserIsProjectAdmin(int projectId);
        Task VerifyUserIsOrganizationAdmin(int organizationId);
    }
}
