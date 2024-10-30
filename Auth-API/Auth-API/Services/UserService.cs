using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;
using System.Linq;

namespace Auth_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserProjectRepository _userProjectRepository;
        private readonly ITokenHandler _tokenHandler;

        public UserService(
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IUserProjectRepository userProjectRepository,
            ITokenHandler tokenHandler)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _userProjectRepository = userProjectRepository;
            _tokenHandler = tokenHandler;
        }

        public async Task<GetUserTokenResposne> Create(CreateUserRequest request)
        {
            var result = new CreateUserValidator().Validate(request);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            var user = new User();
            user.Name = request.Name;
            user.Email = request.Email;
            user.Password = request.Password;

            await _userRepository.Add(user);
            await _userRepository.Commit();

            return new() { Token = _tokenHandler.Generate(user) };
        }

        public async Task<GetUserTokenResposne> Login(LoginRequest request)
        {
            var user = await _userRepository.GetSingle(user => user.Email == request.Email && user.Password == request.Password);

            if (user == null)
                throw new BadRequestException("Incorrect credentials");

            return new() { Token = _tokenHandler.Generate(user) };
        }

        public async Task<IEnumerable<UserResponse>> GetMany(GetManyUsersRequest request)
        {
            var users = await _userRepository.GetAll(user =>
                (!request.Id.HasValue || user.Id == request.Id.Value) &&
                (string.IsNullOrEmpty(request.Name) || user.Name.Contains(request.Name)) &&
                (string.IsNullOrEmpty(request.Email) || user.Email == request.Email) &&
                (!request.ProjectId.HasValue || user.UserProjects.Select(userProject => userProject.ProjectId).Contains(request.ProjectId.Value)) &&
                (!request.RoleId.HasValue || user.RoleUsers.Select(userRole => userRole.RoleId).Contains(request.RoleId.Value)));

            return users.Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Projects = user.UserProjects.Select(userProject => userProject.Project).Select(project => new ProjectForUserResponse
                {
                    Id = project.Id,
                    Name = project.Name
                }),
                Roles = user.RoleUsers.Select(roleUser => roleUser.Role).Select(role => new RoleForUserResponse
                {
                    Id = role.Id,
                    Name = role.Name
                })
            });
        }

        public async Task Delete(int userId)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if(user == null)
                throw new BadRequestException("User not found");

            if(user.UserProjects.Any() || user.RoleUsers.Any())
                throw new BadRequestException("You must remove this users from all roles and project");

            await _userRepository.Delete(user);
            await _userRepository.Commit();
        }

        public async Task AddToProjects(int userId, List<int> projectIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            projectIds = projectIds.Distinct().ToList();

            var linkedProjectIds = user.UserProjects
                .Select(userProject => userProject.ProjectId)
                .Intersect(projectIds)
                .Distinct()
                .ToList();

            if (linkedProjectIds.Any())
                throw new BadRequestException($"User is already linked with the projects: {string.Join(", ", linkedProjectIds)}");

            var projects = await _projectRepository.GetAll(project => projectIds.Contains(project.Id));

            if(projectIds.Count != projects.Count())
                throw new BadRequestException($"Some informed projects was not found");

            var newUserProjects = projects.Select(project => new UserProject { User = user, Project = project });

            await _userProjectRepository.Add(newUserProjects);
            await _userProjectRepository.Commit();
        }

        public async Task RemoveFromProjects(int userId, List<int> projectIds)
        {
            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new BadRequestException("User not found");

            var distinctProjectIds = projectIds.Distinct().ToList();

            var userProjectIds = user.UserProjects.Select(up => up.ProjectId).ToHashSet();
            if (distinctProjectIds.Any(id => !userProjectIds.Contains(id)))
                throw new BadRequestException("User is not linked in all informed projects");

            var userProjectsToRemove = user.UserProjects
                .Where(up => distinctProjectIds.Contains(up.ProjectId))
                .ToList();

            var projectsWithSingleAdmin = userProjectsToRemove
                .Where(up => up.Project.Roles
                    .Any(role => role.Name == EDefaultRole.Admin.GetDescription() &&
                                 role.RoleUsers.Count == 1 &&
                                 role.RoleUsers.Any(roleUser => roleUser.UserId == userId)))
                .ToList();

            if (projectsWithSingleAdmin.Any())
                throw new BadRequestException("User is the only admin on some projects and cannot be removed.");

            var userProjectsIdsToRemove = userProjectsToRemove.Select(userProject =>  userProject.ProjectId).ToList();

            await _userProjectRepository
                .DeleteWhere(userProject => userProject.UserId == userId 
                                         && userProjectsIdsToRemove.Contains(userProject.ProjectId));

            await _userProjectRepository.Commit();
        }
    }

    public interface IUserService
    {
        Task<GetUserTokenResposne> Create(CreateUserRequest request);
        Task<GetUserTokenResposne> Login(LoginRequest request);
        Task<IEnumerable<UserResponse>> GetMany(GetManyUsersRequest request);
        Task Delete(int userId);
        Task AddToProjects(int userId, List<int> projectIds);
        Task RemoveFromProjects(int userId, List<int> projectIds);
    }
}
