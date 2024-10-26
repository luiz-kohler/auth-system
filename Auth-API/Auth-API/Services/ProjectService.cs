﻿using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;
using Microsoft.AspNetCore.Http.HttpResults;
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

            var admin = await GetAdminById(request.AdminId);
            var project = await CreateProject(request);
            var endpoints = await CreateProjectEndpoints(request, project);
            await CreateadminProjectRelationship(admin, project);
            var adminRole = await CreateAdminRole(project);
            await AssignAdminToAdminRole(admin, adminRole);
            await AssignAdminRoleToProjectEndpoints(adminRole, endpoints);

            await _projectRepository.Commit();
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
                throw new BadRequestException("There is already a project with the same name");

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

        private async Task CreateadminProjectRelationship(User admin, Project project)
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
    }

    public interface IProjectService
    {
        Task Create(CreateProjectRequest request);
    }
}
