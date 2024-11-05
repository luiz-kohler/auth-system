namespace Auth_Background_Service
{
    internal class AuthApiClient : HttpClientBase
    {
        public AuthApiClient() : base() { }

        public async Task<string> Login(LoginRequest request)
        {
            var response = await PostAsync<LoginRequest, LoginResponse>($"{EnvVariables.BASE_AUTH_URL}/users/login", request);
            return response.Token;
        }

        public async Task Upsert(UpsertProjectRequest request, string token)
        {
            await PostAsync<UpsertProjectRequest, object>($"{EnvVariables.BASE_AUTH_URL}/projects/upsert", request, token);
        }
    }
}
