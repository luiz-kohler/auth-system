using System.ComponentModel;

namespace Auth_Background_Service
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public int AdminId { get; set; }
        public List<EndpointForCreateProject> Endpoints { get; set; }
    }

    public class EndpointForCreateProject
    {
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
    }

    public enum EHttpMethod
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
