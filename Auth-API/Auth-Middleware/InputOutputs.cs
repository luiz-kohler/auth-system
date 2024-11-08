using System.ComponentModel;
using static Auth_Middleware.AuthMiddleware;

namespace Auth_Middleware
{
    internal class GetManyEndpointRequest
    {
        public string Route { get; set; }
        public EHttpMethod? HttpMethod { get; set; }
        public string ProjectName { get; set; }
    }

    internal class EndpointResponse
    {
        public int Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    internal class VerifyUserHasAccessResponse
    {
        public bool HasAccess { get; set; }
    }

    internal class VerifyUserHasAccessRequest
    {
        public int EndpointId { get; set; }
    }
}
