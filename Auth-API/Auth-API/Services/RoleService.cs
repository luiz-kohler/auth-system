using Auth_API.Common;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;

namespace Auth_API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleUserRepository _roleUserRepository;
        private readonly IRoleEndpointRepository _roleEndpointRepository;

        public RoleService(
            IProjectRepository projectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository)
        {
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _roleEndpointRepository = roleEndpointRepository;
        }
        public async Task Create(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                throw new BadRequestException("You must inform a valid");

            var project = await _projectRepository.GetSingle(project => project.Id == request.ProjectId);
            if(project == null)
                throw new BadRequestException("Project not found");

            var isThereRoleWithSameName = project.Roles.Any(role => role.Name == request.Name);
            if(isThereRoleWithSameName)
                throw new BadRequestException("There is already a role with the same name in this project");

            var role = new Role
            {
                Name = request.Name,
                ProjectId = request.ProjectId
            };

            await _roleRepository.Add(role);
            await _roleRepository.Commit();
        }

        public async Task Delete(List<int> ids)
        {
            if (ids.Count != ids.Distinct().Count())
                throw new BadRequestException("You can not inform repeated roles");

            var roles = await _roleRepository.GetAll(role => ids.Contains(role.Id));

            if (ids.Count != roles.Count())
                throw new BadRequestException("Some roles were not found");

            if(roles.Any(role => role.Name == EDefaultRole.Admin.GetDescription()))
                throw new BadRequestException("You can not delete a admin role.");

            var roleUsers = roles.SelectMany(role => role.RoleUsers);
            await _roleUserRepository.Delete(roleUsers);

            var roleEndpoints = roles.SelectMany(role => role.RoleEndpoints);
            await _roleEndpointRepository.Delete(roleEndpoints);

            await _roleRepository.Delete(roles);
        }
    }

    public interface IRoleService
    {
        Task Create(CreateRoleRequest request);
        Task Delete(List<int> ids);
    }
}