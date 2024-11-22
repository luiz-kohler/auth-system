using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using System.Data;

namespace Auth_API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleUserRepository _roleUserRepository;
        private readonly IRoleEndpointRepository _roleEndpointRepository;
        private readonly IEndpointRepository _endpointRepository;
        private readonly IUserRepository _userRepository;

        public RoleService(
            IProjectRepository projectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository,
            IEndpointRepository endpointRepository,
            IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _roleEndpointRepository = roleEndpointRepository;
            _endpointRepository = endpointRepository;
            _userRepository = userRepository;
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task Create(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                throw new BadRequestException("You must inform a valid name");

            var project = await _projectRepository.GetSingle(project => project.Id == request.ProjectId);
            if (project == null)
                throw new BadRequestException("Project not found");

            var isThereRoleWithSameName = project.Roles.Any(role => role.Name == request.Name);
            if (isThereRoleWithSameName)
                throw new BadRequestException("There is already a role with the same name in this project");

            var role = new Role
            {
                Name = request.Name,
                ProjectId = request.ProjectId
            };

            await _roleRepository.Add(role);
            await _roleRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task Delete(List<int> ids)
        {
            if (ids.Count != ids.Distinct().Count())
                throw new BadRequestException("You can not inform repeated roles");

            var roles = await _roleRepository.GetAll(role => ids.Contains(role.Id));

            if (ids.Count != roles.Count())
                throw new BadRequestException("Some roles were not found");

            if (roles.Any(role => role.Name == EDefaultRole.Admin.GetDescription()))
                throw new BadRequestException("You can not delete an admin role");

            if (roles.Select(role => role.ProjectId).Distinct().Count() > 1)
                throw new BadRequestException("You can not delete roles from different projects in the same operation");

            var roleUsers = roles.SelectMany(role => role.RoleUsers);
            await _roleUserRepository.Delete(roleUsers);

            var roleEndpoints = roles.SelectMany(role => role.RoleEndpoints);
            await _roleEndpointRepository.Delete(roleEndpoints);

            await _roleRepository.Delete(roles);
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task AddEndpoints(int id, List<int> endpointIds)
        {
            if (endpointIds.Count == 0)
                throw new BadRequestException("You must inform any endpoint");

            if (endpointIds.Count != endpointIds.Distinct().Count())
                throw new BadRequestException("You can not inform repeated endpoints");

            var endpoints = await _endpointRepository.GetAll(endpoint => endpointIds.Contains(endpoint.Id));

            if (endpoints.Count() != endpointIds.Count)
                throw new BadRequestException("Some endpoints were not found");

            var role = await _roleRepository.GetSingle(role => role.Id == id);

            if (role == null)
                throw new BadRequestException("Roles was not found");

            if (endpoints.Any(endpoint => endpoint.ProjectId != role.ProjectId))
                throw new BadRequestException($"All endpoint must be from the project: {role.Project.Name}");

            if (role.RoleEndpoints?.Count > 0 &&
                role.RoleEndpoints.Any(roleEndpoint => endpointIds.Contains(roleEndpoint.EndpointId) && roleEndpoint.RoleId == role.Id))
                throw new BadRequestException("Some endpoint is already linked to this role");

            var roleEndpoints = endpoints.Select(endpoint => new RoleEndpoint
            {
                EndpointId = endpoint.Id,
                RoleId = role.Id,
            });

            await _roleEndpointRepository.Add(roleEndpoints);
            await _roleRepository.Commit();
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task RemoveEndpoints(int id, List<int> endpointIds)
        {
            if (endpointIds.Count == 0)
                throw new BadRequestException("You must inform any endpoint");

            if (endpointIds.Count != endpointIds.Distinct().Count())
                throw new BadRequestException("You can not inform repeated endpoints");

            var endpoints = await _endpointRepository.GetAll(endpoint => endpointIds.Contains(endpoint.Id));

            if (endpoints.Count() != endpointIds.Count)
                throw new BadRequestException("Some endpoints were not found");

            var role = await _roleRepository.GetSingle(role => role.Id == id);

            if (role == null)
                throw new BadRequestException("Roles was not found");

            if (role.Name == EDefaultRole.Admin.GetDescription())
                throw new BadRequestException("You can not update the admin role");

            var roleEndpoints = role.RoleEndpoints.Where(roleEndpoint => endpointIds.Contains(roleEndpoint.EndpointId));
            var endpointFromRoleIds = roleEndpoints.Select(roleEndpoint => roleEndpoint.EndpointId);

            if (endpointIds.Any(projectId => !endpointFromRoleIds.Contains(projectId)))
                throw new BadRequestException("Some endpoints are not linked to this role");

            await _roleEndpointRepository.Delete(roleEndpoints);
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task<IEnumerable<RoleResponse>> GetMany(GetManyRolesRequest request)
        {
            var roles = await _roleRepository.GetAll(role =>
                (!request.Id.HasValue || role.Id == request.Id.Value) &&
                (string.IsNullOrEmpty(request.Name) || role.Name.Contains(request.Name)) &&
                (!request.ProjectId.HasValue || role.Project.Id == request.ProjectId.Value) &&
                (!request.EndpointId.HasValue || role.RoleEndpoints.Select(roleEndpoints => roleEndpoints.EndpointId).Contains(request.EndpointId.Value)) &&
                (!request.UserId.HasValue || role.RoleUsers.Select(roleUsers => roleUsers.UserId).Contains(request.UserId.Value)));

            return roles.Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Project = new ProjectForRoleResponse
                {
                    Id = role.Project.Id,
                    Name = role.Project.Name
                }
            });
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task<RoleWithRelationsResponse> Get(int id)
        {
            var role = await _roleRepository.GetSingle(role => role.Id == id);

            if (role == null)
                throw new BadRequestException("Role not found");

            return new RoleWithRelationsResponse
            {
                Id = role.Id,
                Name = role.Name,
                Project = new ProjectForRoleResponse
                {
                    Id = role.Project.Id,
                    Name = role.Project.Name
                },
                Endpoints = role.RoleEndpoints.Select(re => re.Endpoint).Select(endpoint => new EndpointForRoleResponse
                {
                    Id = endpoint.Id,
                    Route = endpoint.Route,
                    HttpMethod = endpoint.HttpMethod,
                    IsPublic = endpoint.IsPublic
                }) ?? new List<EndpointForRoleResponse>(),
                Users = role.RoleUsers.Select(ru => ru.User).Select(user => new UserForRoleResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                })
            };
        }

        public async Task LinkUsers(List<int> userIds, List<int> roleIds)
        {
            var roles = await GetRoles(roleIds);
            if (!roles.Any())
                throw new BadRequestException("Roles not found");

            var users = await GetUsers(userIds);
            if (!users.Any())
                throw new BadRequestException("Users not found");

            var rolesProjectIds = roles.Select(role => role.ProjectId).ToHashSet();

            var validationMessages = users
                .Select(user =>
                {
                    var userProjectIds = user.UserProjects.Select(up => up.ProjectId).ToHashSet();
                    var missingProjects = rolesProjectIds.Except(userProjectIds).ToList();

                    return missingProjects.Any()
                        ? $"User {user.Id} is missing projects: {string.Join(", ", missingProjects)}"
                        : null;
                })
                .Where(message => message != null)
                .ToList();

            if (validationMessages.Any())
                throw new BadRequestException($"Validation failed for the following users:\n{string.Join("\n", validationMessages)}");

            var roleUsers = users
                .SelectMany(user =>
                    roles.Where(role => !user.RoleUsers.Any(ru => ru.UserId == user.Id && ru.RoleId == role.Id))
                         .Select(role => new RoleUser { RoleId = role.Id, UserId = user.Id }))
                .ToList();

            if (!roleUsers.Any())
                throw new BadRequestException("All users are already linked to these roles");

            await _roleUserRepository.Add(roleUsers);
            await _roleUserRepository.Commit();
        }

        public async Task UnlinkUsers(List<int> userIds, List<int> roleIds)
        {
            var roles = await GetRoles(roleIds);
            if (!roles.Any())
                throw new BadRequestException("Roles not found");

            var users = await GetUsers(userIds);
            if (!users.Any())
                throw new BadRequestException("Users not found");

            var rolesUsersToRemove = users.SelectMany(user => user.RoleUsers).Where(ru => roleIds.Contains(ru.RoleId));

            if(!rolesUsersToRemove.Any())
                return;

            await _roleUserRepository.Delete(rolesUsersToRemove);
            await _roleUserRepository.Commit();
        }

        private async Task<IEnumerable<Role>> GetRoles(List<int> roleIds)
        {
            roleIds = roleIds.Distinct().ToList();

            var roles = await _roleRepository.GetAll(role => roleIds.Contains(role.Id));

            if (roleIds.Count != roles.Count())
                throw new BadRequestException($"Some informed roles was not found");

            return roles;
        }

        private async Task<IEnumerable<User>> GetUsers(List<int> userIds)
        {
            userIds = userIds.Distinct().ToList();

            var users = await _userRepository.GetAll(user => userIds.Contains(user.Id));

            if (userIds.Count != users.Count())
                throw new BadRequestException($"Some informed users was not found");

            return users;
        }
    }

    public interface IRoleService
    {
        Task Create(CreateRoleRequest request);
        Task Delete(List<int> ids);
        Task AddEndpoints(int id, List<int> endpointIds);
        Task RemoveEndpoints(int id, List<int> endpointIds);
        Task<IEnumerable<RoleResponse>> GetMany(GetManyRolesRequest request);
        Task<RoleWithRelationsResponse> Get(int id);
        Task LinkUsers(List<int> userIds, List<int> roleIds);
        Task UnlinkUsers(List<int> userIds, List<int> roleIds);
    }
}