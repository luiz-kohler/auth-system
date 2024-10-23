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
    public interface IUserRepository : IBaseEntityRepository<User> { }
}
