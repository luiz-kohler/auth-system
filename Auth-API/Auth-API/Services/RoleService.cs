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

        public RoleService(
            IProjectRepository projectRepository,
            IRoleRepository roleRepository)
        {
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
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
    }

    public interface IRoleService
    {
        Task Create(CreateRoleRequest request);
    }
}
