using Auth_API.Common;

namespace Auth_API.Entities
{
    public class RefreshToken : IBaseEntity
    {
        public int Id { get; set; }
        public string TokenHashed { get; set; }
        public int TimesUsed { get; set; }
        public DateTime LastTimeUsed { get; set; }
        public bool Valid { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
