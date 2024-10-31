using Auth_API.Common;

namespace Auth_API.DTOs
{
    public class CreateEndpointRequest
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }
}
