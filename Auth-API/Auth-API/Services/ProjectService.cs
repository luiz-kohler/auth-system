using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Infra;
using Auth_API.Repositories;
using Auth_API.Validator;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IEndpointRepository _endpointRepository;
        private readonly IUserProjectRepository _userProjectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleUserRepository _roleUserRepository;
        private readonly IRoleEndpointRepository _roleEndpointRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IEndpointRepository endpointRepository,
            IUserProjectRepository userProjectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _endpointRepository = endpointRepository;
            _userProjectRepository = userProjectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _roleEndpointRepository = roleEndpointRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Create(CreateProjectRequest request)
        {
            ValidateRequest(request);

            var adminId = GetUserIdByContext();
            var admin = await GetAdminById(adminId);

            var project = await CreateProject(request);
            var endpoints = await CreateProjectEndpoints(request, project);

            await CreateAdminProjectRelationship(admin, project);

            var adminRole = await CreateAdminRole(project);

            await AssignAdminToAdminRole(admin, adminRole);
            await AssignAdminRoleToProjectEndpoints(adminRole, endpoints);

            await _projectRepository.Commit();
        }

        private int GetUserIdByContext()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.First(c => c.Type == TokenClaimTypes.NameIdentifier)?.Value;

            return Convert.ToInt32(userId);
        }

        private void ValidateRequest(CreateProjectRequest request)
        {
            var result = new CreateProjectValidator().Validate(request);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        private async Task<User> GetAdminById(int adminId)
        {
            var admin = await _userRepository.GetSingle(user => user.Id == adminId);

            if (admin == null)
                throw new NotFoundException("User admin not found");

            return admin;
        }

        private async Task<Project> CreateProject(CreateProjectRequest request)
        {
            var isThereAnyProjectWithSameName = await _projectRepository.Any(project => project.Name == request.Name);

            if (isThereAnyProjectWithSameName)
                throw new BadRequestException("There is already a project with the informed name");

            var project = new Project
            {
                Name = request.Name
            };

            await _projectRepository.Add(project);
            return project;
        }

        private async Task<List<Endpoint>> CreateProjectEndpoints(CreateProjectRequest request, Project project)
        {
            var endpoints = request.Endpoints.Select(endpoint => new Endpoint
            {
                Route = endpoint.Route,
                HttpMethod = endpoint.HttpMethod,
                IsPublic = endpoint.IsPublic,
                Project = project
            }).ToList();

            await _endpointRepository.Add(endpoints);
            return endpoints;
        }

        private async Task CreateAdminProjectRelationship(User admin, Project project)
        {
            var userProject = new UserProject
            {
                Project = project,
                User = admin
            };

            await _userProjectRepository.Add(userProject);
        }

        private async Task<Role> CreateAdminRole(Project project)
        {
            var adminRole = new Role
            {
                Project = project,
                Name = EDefaultRole.Admin.GetDescription()
            };

            await _roleRepository.Add(adminRole);
            return adminRole;
        }

        private async Task AssignAdminToAdminRole(User admin, Role adminRole)
        {
            var roleUser = new RoleUser
            {
                User = admin,
                Role = adminRole
            };

            await _roleUserRepository.Add(roleUser);
        }

        private async Task AssignAdminRoleToProjectEndpoints(Role adminRole, List<Endpoint> endpoints)
        {
            var roleEndpoints = endpoints.Select(endpoint => new RoleEndpoint
            {
                Endpoint = endpoint,
                Role = adminRole
            }).ToList();

            await _roleEndpointRepository.Add(roleEndpoints);
        }

        public async Task<IEnumerable<ProjectToGetManyResponse>> GetMany()
        {
            var projects = await _projectRepository.GetAll();

            return projects.Select(project => new ProjectToGetManyResponse
            {
                Id = project.Id,
                Name = project.Name
            });
        }

        public async Task<GetProjectByIdResponse> Get(int id)
        {
            var project = await _projectRepository.GetSingle(project => project.Id == id);

            if (project == null)
                throw new BadRequestException("Project not found");

            return new()
            {
                Id = project.Id,
                Name = project.Name,
                Users = project.UserProjects.Select(userProject => userProject.User).Select(user => new UserToGetProjectByIdResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                }),
                Roles = project.Roles.Select(role => new RoleToGetProjectByIdResponse
                {
                    Id = role.Id,
                    Name = role.Name
                }),
                Endpoints = project.Endpoints.Select(endpoint => new EndpointToGetProjectByIdResponse
                {
                    Id = endpoint.Id,
                    HttpMethod = endpoint.HttpMethod,
                    IsPublic = endpoint.IsPublic,
                    Route = endpoint.Route
                })
            };
        }

        public async Task Delete(int id)
        {
            var project = await _projectRepository.GetSingle(project => project.Id == id);

            if (project == null)
                throw new BadRequestException("Project not found");

            await RemoveEndpoints(project);
            await RemoveRolesFromProject(project);
            await RemoveUsersFromProject(project);

            await _projectRepository.Delete(project);
        }

        private async Task RemoveEndpoints(Project project)
        {
            var rolesEndpoints = project.Endpoints.SelectMany(endpoint => endpoint.RoleEndpoints);
            if (rolesEndpoints.Any())
                await _roleEndpointRepository.Delete(rolesEndpoints);

            var endpoints = project.Endpoints;
            if (endpoints.Any())
                await _endpointRepository.Delete(endpoints);
        }

        private async Task RemoveUsersFromProject(Project project)
        {
            var userProjects = project.UserProjects;
            if (userProjects.Any())
                await _userProjectRepository.Delete(userProjects);
        }

        private async Task RemoveRolesFromProject(Project project)
        {
            var roleUsers = project.Roles?.SelectMany(role => role?.RoleUsers);
            if(roleUsers.Any())
                await _roleUserRepository.Delete(roleUsers);

            var roles = project.Roles;
            if (roles.Any())
                await _roleRepository.Delete(roles);
        }

        public async Task Upsert(UpsertProjectRequest request)
        {
            var project = await _projectRepository.GetSingle(p => p.Name == request.Name);

            if (project == null)
            {
                await Create(request);
                return;
            }

            var userId = GetUserIdByContext();
            var user = await _userRepository.GetSingle(u => u.Id == userId)
                       ?? throw new BadRequestException("User not found");

            var adminRole = GetAdminRole(project);

            if (IsUserAuthorizedToUpsert(project, userId, adminRole.Id))
            {
                await UpdateProjectEndpoints(project, request, adminRole);
            }
            else
            {
                throw new BadRequestException("You do not have authorization to upsert this project");
            }
        }

        private async Task UpdateProjectEndpoints(Project project, UpsertProjectRequest request, Role adminRole)
        {
            var endpointsToRemove = project.Endpoints
                .Where(currentEndpoint => !request.Endpoints.Any(newEndpoint =>
                    newEndpoint.Route == currentEndpoint.Route &&
                    newEndpoint.HttpMethod == currentEndpoint.HttpMethod &&
                    newEndpoint.IsPublic == currentEndpoint.IsPublic));

            var endpointsToCreate = request.Endpoints
                .Where(newEndpoint => !project.Endpoints.Any(currentEndpoint =>
                    currentEndpoint.Route == newEndpoint.Route &&
                    currentEndpoint.HttpMethod == newEndpoint.HttpMethod &&
                    currentEndpoint.IsPublic == newEndpoint.IsPublic));

            await RemoveEndpointsFromProject(project, endpointsToRemove);
            var newEndpoints = await CreateEndpointsForProject(request, project);
            await AssignAdminRoleToNewEndpoints(adminRole, newEndpoints);
        }

        private async Task RemoveEndpointsFromProject(Project project, IEnumerable<Endpoint> endpointsToRemove)
        {
            if (!endpointsToRemove.Any()) return;

            var roleEndpointsToDelete = endpointsToRemove.SelectMany(e => e.RoleEndpoints);
            if (roleEndpointsToDelete.Any())
            {
                await _roleEndpointRepository.Delete(roleEndpointsToDelete);
            }

            await _endpointRepository.Delete(endpointsToRemove);
        }

        private async Task<List<Endpoint>> CreateEndpointsForProject(UpsertProjectRequest request, Project project)
        {
            var endpoints = request.Endpoints
                .Select(e => new Endpoint
                {
                    Route = e.Route,
                    HttpMethod = e.HttpMethod,
                    IsPublic = e.IsPublic,
                    Project = project
                })
                .ToList();

            await _endpointRepository.Add(endpoints);
            return endpoints;
        }

        private async Task AssignAdminRoleToNewEndpoints(Role adminRole, List<Endpoint> newEndpoints)
        {
            var roleEndpoints = newEndpoints.Select(endpoint => new RoleEndpoint
            {
                Endpoint = endpoint,
                Role = adminRole
            }).ToList();

            await _roleEndpointRepository.Add(roleEndpoints);
        }

        private bool IsUserAuthorizedToUpsert(Project project, int userId, int adminRoleId)
        {
            return project.UserProjects
                .Select(up => up.User)
                .Any(user => user.Id == userId && user.RoleUsers.Any(ru => ru.RoleId == adminRoleId));
        }

        private Role GetAdminRole(Project project)
        {
            return project.Roles.FirstOrDefault(r => r.Name == EDefaultRole.Admin.GetDescription())
                   ?? throw new InvalidOperationException("Admin role not found in the project.");
        }
    }

    public interface IProjectService
    {
        Task Create(CreateProjectRequest request);
        Task Upsert(UpsertProjectRequest request);
        Task<IEnumerable<ProjectToGetManyResponse>> GetMany();
        Task<GetProjectByIdResponse> Get(int id);
        Task Delete(int id);
    }
}
