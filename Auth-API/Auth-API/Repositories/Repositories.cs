using Auth_API.Common;
using Auth_API.Entities;
using Auth_API.Infra;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Repositories
{
    public class EndpointRepository : BaseEntityRepository<Endpoint>, IEndpointRepository
    {
        public EndpointRepository(Context context) : base(context) { }
    }

    public class ProjectRepository : BaseEntityRepository<Project>, IProjectRepository
    {
        public ProjectRepository(Context context) : base(context) { }
    }

    public class RoleRepository : BaseEntityRepository<Role>, IRoleRepository
    {
        public RoleRepository(Context context) : base(context) { }
    }

    public class RoleEndpointRepository : BaseEntityRepository<RoleEndpoint>, IRoleEndpointRepository
    {
        public RoleEndpointRepository(Context context) : base(context) { }
    }

    public class RoleUserRepository : BaseEntityRepository<RoleUser>, IRoleUserRepository
    {
        public RoleUserRepository(Context context) : base(context) { }
    }

    public class UserRepository : BaseEntityRepository<User>, IUserRepository
    {
        public UserRepository(Context context) : base(context) { }

        public async Task<bool> UserHasAccess(int userId, int endpointId)
        {
            var rolesWithAccess = await _context
                .Set<RoleEndpoint>()
                .Where(roleEndpoint => roleEndpoint.EndpointId == endpointId)
                .ToListAsync();

            if(!rolesWithAccess.Any())
                return false;

            return await _context
                .Set<RoleUser>()
                .AnyAsync(roleUser => roleUser.UserId == userId && rolesWithAccess.Select(role => role.EndpointId).Contains(endpointId));
        }

        public override async Task<IEnumerable<User>> GetAll(Expression<Func<User, bool>> predicate)
        {
            return await _context.Set<User>()
                .Include(user => user.UserProjects)
                    .ThenInclude(userProject => userProject.Project)
                .Include(user => user.RoleUsers)
                    .ThenInclude(userRole => userRole.Role)
                .Where(predicate).ToListAsync();
        }
    }

    public class UserProjectRepository : BaseEntityRepository<UserProject>, IUserProjectRepository
    {
        public UserProjectRepository(Context context) : base(context) { }
    }
}
