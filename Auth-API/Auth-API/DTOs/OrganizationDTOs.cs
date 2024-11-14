namespace Auth_API.DTOs
{
    public class CreateOrganizationRequest
    {
        public string Name { get; set; }
    }

    public class LinkUserToOraganizationRequest
    {
        public List<int> UserIds { get; set; }
    }

    public class UnlinkUserToOraganizationRequest
    {
        public List<int> UserIds { get; set; }
    }
}
