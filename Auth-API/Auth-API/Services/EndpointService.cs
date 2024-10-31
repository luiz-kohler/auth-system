using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;
using Azure.Core;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Services
{
    public class EndpointService : IEndpointService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IEndpointRepository _endpointRepository;
        private readonly IRoleEndpointRepository _roleEndpointRepository;

        public EndpointService(
            IProjectRepository projectRepository,
            IEndpointRepository endpointRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository)
        {
            _projectRepository = projectRepository;
            _endpointRepository = endpointRepository;
            _roleEndpointRepository = roleEndpointRepository;
        }

        public async Task Create(List<CreateEndpointRequest> request, int projectId)
        {
            ValidateRequests(request);

            var project = await GetProjectByIdAsync(projectId);

            var repeatedEndpoints = FindRepeatedEndpoints(project, request);
            if (repeatedEndpoints.Any())
                HandleRepeatedEndpoints(repeatedEndpoints);

            var newEndpoints = MapRequestsToEndpoints(request, projectId);
            await _endpointRepository.Add(newEndpoints);

            var newRoleEndpoints = CreateRoleEndpointsForAdmin(project, newEndpoints);
            await _roleEndpointRepository.Add(newRoleEndpoints);

            await _endpointRepository.Commit();
        }

        private void ValidateRequests(List<CreateEndpointRequest> requests)
        {
            foreach (var endpointRequest in requests)
            {
                var result = new CreateEndpointValidator().Validate(endpointRequest);
                if (!result.IsValid)
                    throw new ValidationException(result.Errors);
            }
        }

        private async Task<Project> GetProjectByIdAsync(int projectId)
        {
            var project = await _projectRepository.GetSingle(p => p.Id == projectId);
            if (project == null)
                throw new BadRequestException("Project not found");

            return project;
        }

        private IEnumerable<string> FindRepeatedEndpoints(Project project, List<CreateEndpointRequest> requests)
        {
            var existingEndpoints = project.Endpoints
                .Select(e => (e.Route, e.HttpMethod))
                .ToHashSet();

            return requests
                .Where(r => existingEndpoints.Contains((r.Route, r.HttpMethod)))
                .Select(r => r.Route);
        }

        private void HandleRepeatedEndpoints(IEnumerable<string> repeatedEndpoints)
        {
            var endpointRoutes = string.Join(", ", repeatedEndpoints.Select(endpoint => endpoint));
            throw new BadRequestException($"The following endpoints are already registered in the project: {endpointRoutes}");
        }

        private IEnumerable<Endpoint> MapRequestsToEndpoints(List<CreateEndpointRequest> requests, int projectId)
        {
            return requests.Select(r => new Endpoint
            {
                Route = r.Route,
                HttpMethod = r.HttpMethod,
                IsPublic = r.IsPublic,
                ProjectId = projectId
            });
        }

        private IEnumerable<RoleEndpoint> CreateRoleEndpointsForAdmin(Project project, IEnumerable<Endpoint> newEndpoints)
        {
            var adminRole = project.Roles.FirstOrDefault(role => role.Name == EDefaultRole.Admin.GetDescription());
            if (adminRole == null)
                throw new InvalidOperationException("Admin role not found in project");

            return newEndpoints.Select(endpoint => new RoleEndpoint
            {
                RoleId = adminRole.Id,
                Endpoint = endpoint 
            });
        }
    }

    public interface IEndpointService
    {
        Task Create(List<CreateEndpointRequest> request, int projectId);
    }
}
