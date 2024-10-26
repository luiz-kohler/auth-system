using Auth_API.Common;
using Auth_API.Infra;
using Auth_API.Repositories;
using Auth_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<Context>(opt => opt.UseSqlServer(builder.Configuration["ConnectionStringMSSQL"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// repositories
builder.Services.AddScoped(typeof(IBaseEntityRepository<>), typeof(BaseEntityRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleUserRepository, RoleUserRepository>();
builder.Services.AddScoped<IRoleEndpointRepository, RoleEndpointRepository>();
builder.Services.AddScoped<IUserProjectRepository, UserProjectRepository>();

// services 
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// handlers
builder.Services.AddScoped<ITokenHandler, Auth_API.Common.TokenHandler>();


builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = GlobalExceptionHandler.Handle;
    options.AllowStatusCode404Response = true;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<GlobalRoutePrefixMiddleware>($"/{builder.Configuration["Project"]}");
app.UseMiddleware<RoleBasedTokenMiddleware>();

app.UsePathBase(new PathString($"/{builder.Configuration["Project"]}"));
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
