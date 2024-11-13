using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;
using Azure.Core;
using System.Data;
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

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
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
                throw new BadRequestException("Admin role was not found in project");

            return newEndpoints.Select(endpoint => new RoleEndpoint
            {
                RoleId = adminRole.Id,
                Endpoint = endpoint 
            });
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task Delete(List<int> ids)
        {
            if(ids.Count != ids.Distinct().Count())
                throw new BadRequestException("You can not inform repeated endpoints");

            var endpoints = await _endpointRepository.GetAll(endpoint => ids.Contains(endpoint.Id));

            if(ids.Count != endpoints.Count())
                throw new BadRequestException("Some endpoints were not found");

            if (endpoints.Select(endpoint => endpoint.ProjectId).Distinct().Count() > 1)
                throw new BadRequestException("You can not delete endpoints from different projects in the same operation");

            var endpointIds = endpoints.Select(endpoint => endpoint.Id);

            var roleEndpoints = await _roleEndpointRepository.GetAll(roleEndpoint => endpointIds.Contains(roleEndpoint.EndpointId));

            await _roleEndpointRepository.Delete(roleEndpoints);
            await _endpointRepository.Delete(endpoints);
        }

        // TODO: CHECK USER HAS PERMISSION TO DO THIS
        public async Task<IEnumerable<EndpointResponse>> GetMany(GetManyEndpointRequest request)
        {
            var endpoints = await _endpointRepository.GetAll(endpoint =>
                 (!request.Id.HasValue || endpoint.Id == request.Id.Value) &&
                 (string.IsNullOrEmpty(request.Route) || endpoint.Route == request.Route) &&
                 (!request.IsPublic.HasValue || endpoint.IsPublic == request.IsPublic.Value) &&
                 (!request.HttpMethod.HasValue || endpoint.HttpMethod == request.HttpMethod.Value) &&
                 (!request.ProjectId.HasValue || endpoint.ProjectId == request.ProjectId.Value) &&
                 (!request.RoleId.HasValue || endpoint.RoleEndpoints.Any(roleEndpoint => roleEndpoint.RoleId == request.RoleId.Value)) &&
                 (string.IsNullOrEmpty(request.ProjectName) || endpoint.Project.Name == request.ProjectName));

            return endpoints.Select(endpoint => new EndpointResponse
            {
                Id = endpoint.Id,
                Route = endpoint.Route,
                HttpMethod = endpoint.HttpMethod,
                IsPublic = endpoint.IsPublic,
                Project = new ProjectToEndpointResponse { Id = endpoint.ProjectId, Name = endpoint.Project.Name },
                Roles = endpoint.RoleEndpoints.Select(roleEndpoint => roleEndpoint.Role).Select(role => new RoleToEndpointResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                })
            });
        }
    }

    public interface IEndpointService
    {
        Task Create(List<CreateEndpointRequest> request, int projectId);
        Task Delete(List<int> ids);
        Task<IEnumerable<EndpointResponse>> GetMany(GetManyEndpointRequest request);
    }
}
