namespace Auth_API.Handlers
{
    public class HashHandler : IHashHandler
    {
        private readonly string _salt;

        public HashHandler(IConfiguration configuration)
        {
            _salt = configuration["HashSalt"];
            if (string.IsNullOrEmpty(_salt))
                throw new ArgumentException("HashSalt must be informed");
        }

        // Método para gerar o hash de uma string
        public string Hash(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input, _salt);
        }

        public bool Verify(string input, string hashedValue)
        {
            return BCrypt.Net.BCrypt.Verify(input, hashedValue);
        }
    }

    public interface IHashHandler
    {
        string Hash(string input);
        bool Verify(string input, string hashedValue);
    }
}
