using static Auth_Middleware.AuthMiddleware;

namespace Auth_Middleware
{
    internal class AuthApiClient : HttpClientBase
    {
        public AuthApiClient() : base() { }

        public async Task<IEnumerable<EndpointResponse>> GetEndpoints(GetManyEndpointRequest request)
        {
            return await PostAsync<GetManyEndpointRequest, List<EndpointResponse>>($"{EnvVariables.BASE_AUTH_URL}/endpoints/search-many", request);
        }

        public async Task<bool> VerifyIfUserHasAccessToEndpoint(VerifyUserHasAccessRequest request, string token)
        {
            var response = await PostAsync<VerifyUserHasAccessRequest, VerifyUserHasAccessResponse>($"{EnvVariables.BASE_AUTH_URL}/users/has-access-to-endpoint", request, token);
            return response.HasAccess;
        }
    }
}
