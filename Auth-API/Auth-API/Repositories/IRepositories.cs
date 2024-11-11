using Auth_API.Common;
using Auth_API.Entities;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Repositories
{
    public interface IEndpointRepository : IBaseEntityRepository<Endpoint> { }
    public interface IProjectRepository : IBaseEntityRepository<Project> { }
    public interface IRoleRepository : IBaseEntityRepository<Role> { }
    public interface IRoleEndpointRepository : IBaseEntityRepository<RoleEndpoint> { }
    public interface IRoleUserRepository : IBaseEntityRepository<RoleUser> { }
    public interface IUserProjectRepository : IBaseEntityRepository<UserProject> { }
    public interface IRefreshTokenRepository : IBaseEntityRepository<RefreshToken> { }
    public interface IUserRepository : IBaseEntityRepository<User>
    {
        Task<bool> UserHasAccess(int userId, int endpointId);
    }
}
