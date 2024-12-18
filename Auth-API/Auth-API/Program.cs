using Auth_API.Common;
using Auth_API.Handlers;
using Auth_API.Infra;
using Auth_API.Middlewares;
using Auth_API.Repositories;
using Auth_API.Services;
using Auth_Background_Service;
using Auth_Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<Context>(opt => opt.UseSqlServer(builder.Configuration["ConnectionStringMSSQL"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService(provider =>
    new UpsertProjectService(
        new UpsertProjectProfile
        {
            Assembly = Assembly.GetExecutingAssembly(),
            Email = builder.Configuration["AdminEmail"],
            Password = builder.Configuration["AdminPassword"],
            Project = builder.Configuration["Project"]
        }, provider.GetRequiredService<IHostApplicationLifetime>()));

// repositories
builder.Services.AddScoped(typeof(IBaseEntityRepository<>), typeof(BaseEntityRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleUserRepository, RoleUserRepository>();
builder.Services.AddScoped<IRoleEndpointRepository, RoleEndpointRepository>();
builder.Services.AddScoped<IUserProjectRepository, UserProjectRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// services 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEndpointService, EndpointService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();

// handlers
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenHandler, TokenHandler>();
builder.Services.AddScoped<IHashHandler, HashHandler>();
builder.Services.AddScoped<IEncryptHandler, EncryptHandler>();
builder.Services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();

builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = GlobalExceptionHandler.Handle;
    options.AllowStatusCode404Response = true;
});

var app = builder.Build();

app.UseCors(option => option
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .WithMethods("POST")
    .AllowCredentials()
);

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<GlobalRoutePrefixMiddleware>($"/{builder.Configuration["Project"]}");
app.UseMiddleware<AuthMiddleware>();

app.UsePathBase(new PathString($"/{builder.Configuration["Project"]}"));
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
