using Auth_API.Entities;
using Microsoft.EntityFrameworkCore;
using Endpoint = Auth_API.Entities.Endpoint;

namespace Auth_API.Infra
{
    public class Context : DbContext
    {
        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleEndpoint> RolesEndpoints { get; set; }
        public DbSet<RoleUser> RolesUsers { get; set; }
        public DbSet<User> Users { get; set; }

        public Context(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.NoAction;

            modelBuilder.Entity<RoleEndpoint>()
                .HasKey(re => new { re.RoleId, re.EndpointId });

            modelBuilder.Entity<RoleUser>()
                .HasKey(ru => new { ru.RoleId, ru.UserId });
        }
    }
}
