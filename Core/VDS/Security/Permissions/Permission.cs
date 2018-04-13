using System.Collections.Generic;
using System.Security.Claims;
using VDS.Domain.Entities;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;

namespace VDS.Security.Permissions
{
    public class Permission : Entity
    {
        public const string ClaimType = "Permission";

        public Permission()
        {
            _children = new List<Permission>();
        }

        public Permission(string name)
            : this()
        {
            Throw.IfArgumentNull(name, nameof(name));

            Name = name;
        }

        public Permission(string name, string displayName = null, string description = null, Permission[] children = null)
            : this(name)
        {
            DisplayName = displayName;
            Description = description;
            if (children != null)
            {
                children.ForEach(c => c.Parent = this);
                _children.AddRange(children);
            }
        }

        public Permission AddChild(string name, string displayName = null, string description = null)
        {
            Throw.IfArgumentNull(name, nameof(name));
            var permission = new Permission(name, displayName, description)
            {
                Parent = this
            };
            _children.Add(permission);
            return permission;
        }

        public Permission Parent { get; private set; }
        public IReadOnlyList<Permission> Children => _children;
        private readonly List<Permission> _children;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DisplayName { get; set; }

        public static implicit operator Claim(Permission p)
        {
            return new Claim(ClaimType, p.Name);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Permission))
            {
                return false;
            }
            return base.Equals(obj) && Name == ((Permission)obj).Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}