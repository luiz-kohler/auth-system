using System.ComponentModel;

namespace Auth_Background_Service
{
    internal class UpsertProjectRequest
    {
        public string Name { get; set; }
        public List<EndpointForUpsertProject> Endpoints { get; set; }
    }

    internal class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    internal class EndpointForUpsertProject
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    internal class LoginResponse
    {
        public string Token { get; set; }
    }

    internal enum EHttpMethod
    {
        [Description("PUT")]
        PUT,
        [Description("POST")]
        POST,
        [Description("GET")]
        GET,
        [Description("DELETE")]
        DELETE
    }
}
