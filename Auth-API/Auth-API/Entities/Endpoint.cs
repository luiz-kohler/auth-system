﻿using Auth_API.Common;

namespace Auth_API.Entities
{
    public class Endpoint : IBaseEntity
    {
        public int Id { get; set; }
        public string Route { get; set; }
        public EHttpMethod HttpMethod { get; set; }
        public bool IsPublic { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public virtual IReadOnlyCollection<RoleEndpoint> RoleEndpoints { get; set; }
    }
}
