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
        private readonly IHashHandler _hashHandler;
        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITokenHandler tokenHandler,
            IHashHandler hashHandler)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _tokenHandler = tokenHandler;
            _hashHandler = hashHandler;
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

            var refreshTokenHashed = _hashHandler.Hash(refreshToken);
            var refreshTokenEntity = await _refreshTokenRepository.GetSingle(x => x.TokenHashed == refreshTokenHashed);

            if (refreshTokenEntity == null)
                throw new UnauthorizedAccessException();

            if (!refreshTokenEntity.Valid ||
                refreshTokenEntity.TokenHashed != refreshTokenHashed ||
                refreshTokenEntity.TimesUsed > MAX_USED_TIMES ||
                refreshTokenEntity.LastTimeUsed < DateTime.UtcNow.AddMonths(-6))
            {
                refreshTokenEntity.Valid = false;

                await _refreshTokenRepository.Update(refreshTokenEntity);
                await _refreshTokenRepository.Commit();

                throw new UnauthorizedAccessException();
            }

            refreshTokenEntity.LastTimeUsed = DateTime.UtcNow;
            refreshTokenEntity.TimesUsed++;

            await _refreshTokenRepository.Update(refreshTokenEntity);
            await _refreshTokenRepository.Commit();

            return _tokenHandler.Generate(refreshTokenEntity.User);
        }

        public async Task<string> Generate(User user)
        {
            var oldRefreshToken = user.RefreshToken;
            if(oldRefreshToken != null)
                await _refreshTokenRepository.Delete(oldRefreshToken);

            var token = Generate();

            var newRefreshToken = new RefreshToken
            {
                LastTimeUsed = DateTime.UtcNow,
                TimesUsed = 0,
                TokenHashed = _hashHandler.Hash(token),
                Valid = true,
                UserId = user.Id
            };

            await _refreshTokenRepository.Add(newRefreshToken);
            await _refreshTokenRepository.Commit();

            return token;
        }
    }

    public interface IRefreshTokenHandler
    {
        Task<string> Generate(User user);
        Task<string> Refresh(string token, string refreshToken);
    }
}
