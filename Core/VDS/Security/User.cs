using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using VDS.Domain.Entities;

namespace VDS.Security
{
    public class User : Entity<long>, IPassivable
    {
        [Required]
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsLockoutEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public string NormalizedEmail { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        /// <summary>
        /// Login definitions for this user.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ICollection<UserLogin> Logins { get; set; }
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
        public bool IsActive { get; set; }
        public static ClaimsIdentity Identity { get; set; }

        public User()
        {
            IsActive = true;
            SecurityStamp = SequentialGuidGenerator.Instance.Create().ToString();
        }

        public override string ToString()
        {
            return UserName;
        }
    }
}