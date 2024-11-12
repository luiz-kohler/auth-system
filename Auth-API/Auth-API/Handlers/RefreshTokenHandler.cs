using Auth_API.Entities;
using Auth_API.Repositories;
using System.Security.Cryptography;

namespace Auth_API.Handlers
{
    public class RefreshTokenHandler : IRefreshTokenHandler
    {
        private const int MAX_USED_TIMES = 50;

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenHandler _tokenHandler;


        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITokenHandler tokenHandler)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _tokenHandler = tokenHandler;
        }

        public string Generate()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string> Refresh(string token, string refreshToken)
        {
            if (!_tokenHandler.ValidateIgnoringLifeTime(token))
                throw new UnauthorizedAccessException();

            var userId = _tokenHandler.ExtractUserId(token);

            var user = await _userRepository.GetSingle(user => user.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException();

            if (user.RefreshToken == null)
            {
                var newRefreshToken = new RefreshToken
                {
                    LastTimeUsed = DateTime.UtcNow,
                    TimesUsed = 1,
                    Valid = true,
                    TokenHashed = Generate(),
                    UserId = user.Id,
                };

                user.RefreshToken = newRefreshToken;

                await _refreshTokenRepository.Update(newRefreshToken);
            }
            else
            {
                if (!user.RefreshToken.Valid ||
                    user.RefreshToken.TokenHashed != refreshToken ||
                    user.RefreshToken.TimesUsed > MAX_USED_TIMES ||
                    user.RefreshToken.LastTimeUsed < DateTime.UtcNow.AddMonths(-6))
                    throw new UnauthorizedAccessException();

                user.RefreshToken.LastTimeUsed = DateTime.UtcNow;
                user.RefreshToken.TimesUsed++;

                await _refreshTokenRepository.Update(user.RefreshToken);
            }

            await _refreshTokenRepository.Commit();

            return _tokenHandler.Generate(user);
        }
    }

    public interface IRefreshTokenHandler
    {
        string Generate();
        Task<string> Refresh(string token, string refreshToken);
    }
}
