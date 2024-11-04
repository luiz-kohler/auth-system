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
    }

    public interface IProjectService
    {
        Task Create(CreateProjectRequest request);
        Task<IEnumerable<ProjectToGetManyResponse>> GetMany();
        Task<GetProjectByIdResponse> Get(int id);
        Task Delete(int id);
    }
}
