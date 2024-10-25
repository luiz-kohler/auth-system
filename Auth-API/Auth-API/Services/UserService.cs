using Auth_API.Common;
using Auth_API.DTOs;
using Auth_API.Entities;
using Auth_API.Exceptions;
using Auth_API.Repositories;
using Auth_API.Validator;

namespace Auth_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenHandler _tokenHandler;

        public UserService(
            IUserRepository userRepository,
            ITokenHandler tokenHandler)
        {
            _userRepository = userRepository;
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
    }

    public interface IUserService
    {
        Task<GetUserTokenResposne> Create(CreateUserRequest request);
    }
}
