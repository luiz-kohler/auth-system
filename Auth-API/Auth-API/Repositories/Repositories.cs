using Auth_API.Entities;
using Auth_API.Infra;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Repositories
{
    public class EndpointRepository : BaseEntityRepository<Endpoint>, IEndpointRepository
    {
        public EndpointRepository(Context context) : base(context) { }

        public override async Task<IEnumerable<Endpoint>> GetAll(Expression<Func<Endpoint, bool>> predicate)
        {
            return await _context.Set<Endpoint>()
                .Include(user => user.RoleEndpoints)
                    .ThenInclude(roleEndpoint => roleEndpoint.Role)
                .Include(user => user.Project)
                .Where(predicate).ToListAsync();
        }
    }

    public class ProjectRepository : BaseEntityRepository<Project>, IProjectRepository
    {
        public ProjectRepository(Context context) : base(context) { }

        public override async Task<Project> GetSingle(Expression<Func<Project, bool>> predicate)
        {
            return await _context.Set<Project>()
                .Include(project => project.Roles)
                    .ThenInclude(role => role.RoleUsers)
                .Include(project => project.Endpoints)
                    .ThenInclude(endpoint => endpoint.RoleEndpoints)
                .Include(project => project.UserProjects)
                    .ThenInclude(userProjects => userProjects.User)
                .FirstOrDefaultAsync(predicate);
        }
    }

    public class RoleRepository : BaseEntityRepository<Role>, IRoleRepository
    {
        public RoleRepository(Context context) : base(context) { }

        public override async Task<IEnumerable<Role>> GetAll(Expression<Func<Role, bool>> predicate)
        {
            return await _context.Set<Role>()
                         .Include(role => role.RoleEndpoints)
                         .Include(role => role.RoleUsers)
                         .Include(role => role.Project)
                         .Where(predicate)
                         .ToListAsync();
        }

        public override async Task<Role> GetSingle(Expression<Func<Role, bool>> predicate)
        {
            return await _context.Set<Role>()
                         .Include(role => role.RoleEndpoints)
                            .ThenInclude(re => re.Endpoint)
                         .Include(role => role.RoleUsers)
                            .ThenInclude(ru => ru.User)
                         .Include(role => role.Project)
                         .FirstOrDefaultAsync(predicate);
        }
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

        public override async Task<User> GetSingle(Expression<Func<User, bool>> predicate)
        {
            return await _context.Set<User>()
                .Include(user => user.RefreshToken)
                .Include(user => user.UserProjects)
                    .ThenInclude(userProject => userProject.Project)
                        .ThenInclude(project => project.Roles)
                            .ThenInclude(role => role.RoleUsers)
                .Include(user => user.RoleUsers)
                    .ThenInclude(userRole => userRole.Role)
                .FirstOrDefaultAsync(predicate);
        }
    }

    public class UserProjectRepository : BaseEntityRepository<UserProject>, IUserProjectRepository
    {
        public UserProjectRepository(Context context) : base(context) { }
    }

    public class RefreshTokenRepository : BaseEntityRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(Context context) : base(context) { }

        public override async Task<RefreshToken> GetSingle(Expression<Func<RefreshToken, bool>> predicate)
        {
            return await _context.Set<RefreshToken>()
                 .Include(refreshToken => refreshToken.User)
                 .FirstOrDefaultAsync(predicate);
        }
    }
}
