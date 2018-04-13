using System;
using System.Security.Claims;
using VDS.Domain.Entities;

namespace VDS.Security
{
    /// <summary>
    /// Represents a claim that is granted to all users within a role.
    /// </summary>
    public class RoleClaim : Entity<Guid>
    {
        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public string ClaimValue { get; set; }

        public int RoleId { get; set; }

        public Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }
    }
}