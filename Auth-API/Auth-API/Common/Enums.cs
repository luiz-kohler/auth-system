using System.ComponentModel;

namespace Auth_API.Common
{
    public enum EDefaultRole
    {
        [Description("Admin")]
        Admin,
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
