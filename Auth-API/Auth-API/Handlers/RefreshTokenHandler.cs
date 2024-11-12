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

        private string Generate()
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

            var refreshTokenEntity = await _refreshTokenRepository.GetSingle(x => x.TokenHashed == refreshToken);

            if (refreshTokenEntity == null)
                throw new UnauthorizedAccessException();

            if (!refreshTokenEntity.Valid ||
                refreshTokenEntity.TokenHashed != refreshToken ||
                refreshTokenEntity.TimesUsed > MAX_USED_TIMES ||
                refreshTokenEntity.LastTimeUsed < DateTime.UtcNow.AddMonths(-6))
                throw new UnauthorizedAccessException();

            refreshTokenEntity.LastTimeUsed = DateTime.UtcNow;
            refreshTokenEntity.TimesUsed++;

            await _refreshTokenRepository.Update(refreshTokenEntity);
            await _refreshTokenRepository.Commit();

            return _tokenHandler.Generate(refreshTokenEntity.User);
        }

        public async Task<string> Generate(User user)
        {
            var refreshToken = new RefreshToken
            {
                LastTimeUsed = DateTime.UtcNow,
                TimesUsed = 0,
                TokenHashed = Generate(),
                Valid = true,
                UserId = user.Id
            };

            await _refreshTokenRepository.Add(refreshToken);
            await _refreshTokenRepository.Commit();

            return refreshToken.TokenHashed;
        }
    }

    public interface IRefreshTokenHandler
    {
        Task<string> Generate(User user);
        Task<string> Refresh(string token, string refreshToken);
    }
}
