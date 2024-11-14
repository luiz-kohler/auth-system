using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Handlers;
using Auth_API.Repositories;

namespace Auth_API.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IEndpointRepository _endpointRepository;
        private readonly IUserProjectRepository _userProjectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleUserRepository _roleUserRepository;
        private readonly IRoleEndpointRepository _roleEndpointRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserService _userService;

        public OrganizationService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IEndpointRepository endpointRepository,
            IUserProjectRepository userProjectRepository,
            IRoleRepository roleRepository,
            IRoleUserRepository roleUserRepository,
            IRoleEndpointRepository roleEndpointRepository,
            IOrganizationRepository organizationRepository,
            ITokenHandler tokenHandler,
            IUserService userService)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _endpointRepository = endpointRepository;
            _userProjectRepository = userProjectRepository;
            _roleRepository = roleRepository;
            _roleUserRepository = roleUserRepository;
            _roleEndpointRepository = roleEndpointRepository;
            _organizationRepository = organizationRepository;
            _tokenHandler = tokenHandler;
            _userService = userService;
        }

        public async Task Create(CreateOrganizationRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                throw new BadRequestException("Organization name must be informed");

            var user = await _userService.ExtractUserFromCurrentSession();
            if (user == null)
                throw new UnauthorizedAccessException();

            if(user.OrganizationId.HasValue)
                throw new BadRequestException("You are already linked to an organization");

            var organizationWithSameName = await _organizationRepository.GetSingle(organization => organization.Name == request.Name);
            if(organizationWithSameName == null)
                throw new BadRequestException("There are a organization with the same name");

            var organization = new Organization { Name = request.Name };
            await _organizationRepository.Add(organization);

            user.OrganizationId = organization.Id;
            user.IsUserOrganizationAdmin = true;
            await _userRepository.Update(user);

            await _organizationRepository.Commit();
        }

        public async Task LinkUsers(LinkUserToOraganizationRequest request)
        {
            var currentUser = await _userService.ExtractUserFromCurrentSession();
            if (currentUser == null)
                throw new UnauthorizedAccessException();

            if (!currentUser.OrganizationId.HasValue)
                throw new BadRequestException("You must be linked to organization to complete this action");

            if(!currentUser.IsUserOrganizationAdmin.Value)
                throw new UnauthorizedAccessException();

            var userIds = request.UserIds.Distinct();

            var users = await _userRepository.GetAll(user => !user.OrganizationId.HasValue
                                                          && userIds.Contains(user.Id));

            if (users.Count() != userIds.Count())
                throw new BadRequestException("Some users were not found");

            await _userRepository.UpdateMany(user => !user.OrganizationId.HasValue
                                                          && userIds.Contains(user.Id),
                                                          setters => setters
                                                         .SetProperty(user => user.OrganizationId, currentUser.OrganizationId)
                                                         .SetProperty(user => user.IsUserOrganizationAdmin, false));

            await _userRepository.Commit();
        }

        public async Task UnlinkUsers(UnlinkUserToOraganizationRequest request)
        {
            var currentUser = await _userService.ExtractUserFromCurrentSession();
            if (currentUser == null)
                throw new UnauthorizedAccessException();

            if (!currentUser.OrganizationId.HasValue)
                throw new BadRequestException("You must be linked to organization to complete this action");

            if (!currentUser.IsUserOrganizationAdmin.Value)
                throw new UnauthorizedAccessException();

            var userIds = request.UserIds.Distinct();

            var users = await _userRepository.GetAll(user => !user.OrganizationId.HasValue
                                                          && userIds.Contains(user.Id));

            if (users.Count() != userIds.Count())
                throw new BadRequestException("Some users were not found");

            if (users.Any(user => user.OrganizationId != currentUser.OrganizationId))
                throw new BadRequestException("All users must be linked to this organization");

            await _userRepository.UpdateMany(user => !user.OrganizationId.HasValue
                                                          && userIds.Contains(user.Id),
                                                          setters => setters
                                                         .SetProperty(user => user.OrganizationId, (int?)null)
                                                         .SetProperty(user => user.IsUserOrganizationAdmin, (bool?)null));

            await _userRepository.Commit();
        }
    }

    public interface IOrganizationService
    {
        Task Create(CreateOrganizationRequest request);
        Task LinkUsers(LinkUserToOraganizationRequest request);
        Task UnlinkUsers(UnlinkUserToOraganizationRequest request);
    }
}
