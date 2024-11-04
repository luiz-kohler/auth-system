using System.Reflection;

namespace Auth_Background_Service
{
    public class PatchProjectProfile
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Project { get; set; }
        public Assembly Assembly { get; set; }
    }
}
