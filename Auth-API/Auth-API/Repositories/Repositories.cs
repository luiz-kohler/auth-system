using Auth_API.Common;
using Auth_API.Entities;
using Auth_API.Infra;
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
    }

    public class UserProjectRepository : BaseEntityRepository<UserProject>, IUserProjectRepository
    {
        public UserProjectRepository(Context context) : base(context) { }
    }
}
