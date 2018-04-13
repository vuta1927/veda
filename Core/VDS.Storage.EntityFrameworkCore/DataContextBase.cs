using VDS.BackgroundJobs;
using VDS.Data.Uow;
using VDS.Messaging.Events;
using VDS.Notifications;
using VDS.Security;
using VDS.Security.Permissions;
using VDS.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore
{
    public abstract class DataContextBase<TSelf> : DataContext
        where TSelf : DataContextBase<TSelf>
    {
        /// <summary>
        /// Roles
        /// </summary>
        public virtual DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Role Claim
        /// </summary>
        public virtual DbSet<RoleClaim> RoleClaims { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        public virtual DbSet<User> Users { get; set; }

        /// <summary>
        /// User logins.
        /// </summary>
        public virtual DbSet<UserLogin> UserLogins { get; set; }

        /// <summary>
        /// User roles.
        /// </summary>
        public virtual DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Permissions.
        /// </summary>
        public virtual DbSet<Permission> Permissions { get; set; }

        /// <summary>
        /// Settings
        /// </summary>
        public virtual DbSet<Setting> Settings { get; set; }

        /// <summary>
        /// Background jobs.
        /// </summary>
        public virtual DbSet<BackgroundJobInfo> BackgroundJobs { get; set; }

        /// <summary>
        /// Notifications
        /// </summary>
        public virtual DbSet<NotificationInfo> Notifications { get; set; }

        /// <summary>
        /// User notifications.
        /// </summary>
        public virtual DbSet<UserNotificationInfo> UserNotifications { get; set; }

        /// <summary>
        /// Notification subscriptions.
        /// </summary>
        public virtual DbSet<NotificationSubscriptionInfo> NotificationSubscriptions { get; set; }

        /// <summary>
        /// Event logs.
        /// </summary>
        public virtual DbSet<EventLogEntry> EventLogEntries { get; set; }

        protected DataContextBase(DbContextOptions options, ICurrentUnitOfWorkProvider currentUnitOfWorkProvider, IMediator mediator)
            : base(options, currentUnitOfWorkProvider, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Permission>(b => b.HasIndex(e => e.Name));

            modelBuilder.Entity<RoleClaim>(b =>
            {
                b.HasIndex(e => e.Id);
                b.HasIndex(e => e.ClaimType);
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.HasIndex(e => e.NormalizedRoleName);
            });
            
            modelBuilder.Entity<NotificationSubscriptionInfo>(b =>
            {
                b.HasIndex(e => new { e.NotificationName, e.EntityTypeName, e.EntityId, e.UserId });
            });

            modelBuilder.Entity<UserNotificationInfo>(b =>
            {
                b.HasIndex(e => new { e.UserId, e.State, e.CreationTime });
            });

            modelBuilder.Entity<UserRole>(b =>
            {
                b.HasIndex(e => new { e.UserId });
                b.HasIndex(e => new { e.RoleId });
            });

            modelBuilder.Entity<User>(u =>
            {
                u.HasIndex(e => e.NormalizedUserName);
                u.HasIndex(e => e.NormalizedEmail);
            });

            modelBuilder.Entity<UserLogin>(ul =>
            {
                ul.HasIndex(e => new {e.LoginProvider, e.ProviderKey});
                ul.HasIndex(e => e.UserId);
            });
        }
    }
}
