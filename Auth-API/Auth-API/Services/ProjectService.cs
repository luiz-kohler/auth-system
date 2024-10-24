using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;
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

        public ProjectService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IEndpointRepository endpointRepository,
            IUserProjectRepository userProjectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _endpointRepository = endpointRepository;
            _userProjectRepository = userProjectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _roleEndpointRepository = roleEndpointRepository;
        }

        public async Task Create(CreateProjectRequest request)
        {
            ValidateRequest(request);

            var manager = await GetManagerById(request.ManagerId);
            var project = await CreateProject(request);
            var endpoints = await CreateProjectEndpoints(request, project);
            await CreateManagerProjectRelationship(manager, project);
            var adminRole = await CreateAdminRole(project);
            await AssignManagerToAdminRole(manager, adminRole);
            await AssignAdminRoleToProjectEndpoints(adminRole, endpoints);

            await _projectRepository.Commit();
        }

        private void ValidateRequest(CreateProjectRequest request)
        {
            var result = new CreateProjectValidator().Validate(request);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        private async Task<User> GetManagerById(int managerId)
        {
            var manager = await _userRepository.GetSingle(user => user.Id == managerId);

            if (manager == null)
                throw new NotFoundException("User manager not found");

            return manager;
        }

        private async Task<Project> CreateProject(CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name
            };

            await _projectRepository.Add(project);
            return project;
        }

        private async Task<List<Endpoint>> CreateProjectEndpoints(CreateProjectRequest request, Project project)
        {
            var endpoints = request.Endpoints.Select(route => new Endpoint
            {
                Route = route,
                Project = project
            }).ToList();

            await _endpointRepository.Add(endpoints);
            return endpoints;
        }

        private async Task CreateManagerProjectRelationship(User manager, Project project)
        {
            var userProject = new UserProject
            {
                Project = project,
                User = manager
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

        private async Task AssignManagerToAdminRole(User manager, Role adminRole)
        {
            var roleUser = new RoleUser
            {
                User = manager,
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
    }

    public interface IProjectService
    {
        Task Create(CreateProjectRequest request);
    }
}
