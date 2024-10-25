using Auth_API.Common;
using Auth_API.Infra;
using Auth_API.Repositories;
using Auth_API.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<Context>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// repositories
builder.Services.AddTransient(typeof(IBaseEntityRepository<>), typeof(BaseEntityRepository<>));
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
builder.Services.AddTransient<IEndpointRepository, EndpointRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IRoleUserRepository, RoleUserRepository>();
builder.Services.AddTransient<IRoleEndpointRepository, RoleEndpointRepository>();
builder.Services.AddTransient<IUserProjectRepository, UserProjectRepository>();

// services 
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IProjectService, ProjectService>();

// handlers
builder.Services.AddTransient<ITokenHandler, TokenHandler>();


builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = GlobalExceptionHandler.Handle;
    options.AllowStatusCode404Response = true;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
