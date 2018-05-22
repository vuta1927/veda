﻿// <auto-generated />
using ApiServer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using VDS.BackgroundJobs;
using VDS.Messaging.Events;
using VDS.Notifications;

namespace ApiServer.Migrations
{
    [DbContext(typeof(VdsContext))]
    [Migration("20180522062829_modifyTagQc2")]
    partial class modifyTagQc2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ApiServer.Model.Class", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClassColor");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<string>("Note");

                    b.Property<Guid?>("ProjectId");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("ApiServer.Model.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Classes");

                    b.Property<double>("Height");

                    b.Property<bool>("Ignored");

                    b.Property<string>("Path");

                    b.Property<Guid?>("ProjectId");

                    b.Property<DateTime?>("QcDate");

                    b.Property<string>("QcStatus");

                    b.Property<int?>("QuantityCheckId");

                    b.Property<int>("TagHasClass");

                    b.Property<int>("TagNotHasClass");

                    b.Property<double>("TagTime");

                    b.Property<DateTime?>("TaggedDate");

                    b.Property<int>("TotalClass");

                    b.Property<double>("Width");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("QuantityCheckId")
                        .IsUnique()
                        .HasFilter("[QuantityCheckId] IS NOT NULL");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ApiServer.Model.PermissionRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PermissionId");

                    b.Property<int>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("PermissionRoles");
                });

            modelBuilder.Entity("ApiServer.Model.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<bool>("IsDisabled");

                    b.Property<string>("Name");

                    b.Property<string>("Note");

                    b.Property<int?>("ProjectSettingId");

                    b.Property<int>("TotalImg");

                    b.Property<int>("TotalImgNotClassed");

                    b.Property<int>("TotalImgNotQC");

                    b.Property<int>("TotalImgNotTagged");

                    b.Property<int>("TotalImgQC");

                    b.HasKey("Id");

                    b.HasIndex("ProjectSettingId")
                        .IsUnique()
                        .HasFilter("[ProjectSettingId] IS NOT NULL");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("ApiServer.Model.ProjectSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("QuantityCheckLevel");

                    b.Property<double>("TaggTimeValue");

                    b.HasKey("Id");

                    b.ToTable("ProjectSettings");
                });

            modelBuilder.Entity("ApiServer.Model.ProjectUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ProjectId");

                    b.Property<int>("RoleId");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("ProjectUsers");
                });

            modelBuilder.Entity("ApiServer.Model.QuantityCheck", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comment");

                    b.Property<string>("CommentLevel1");

                    b.Property<string>("CommentLevel2");

                    b.Property<string>("CommentLevel3");

                    b.Property<string>("CommentLevel4");

                    b.Property<string>("CommentLevel5");

                    b.Property<DateTime>("QCDate");

                    b.Property<bool?>("Value1");

                    b.Property<bool?>("Value2");

                    b.Property<bool?>("Value3");

                    b.Property<bool?>("Value4");

                    b.Property<bool?>("Value5");

                    b.HasKey("Id");

                    b.ToTable("QuantityChecks");
                });

            modelBuilder.Entity("ApiServer.Model.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ClassId");

                    b.Property<Guid?>("ImageId");

                    b.Property<int>("Index");

                    b.Property<double>("Left");

                    b.Property<int?>("QuantityCheckId");

                    b.Property<DateTime>("TaggedDate");

                    b.Property<double>("Top");

                    b.Property<double>("Width");

                    b.Property<double>("height");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("ImageId");

                    b.HasIndex("QuantityCheckId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("ApiServer.Model.UserQuantityCheck", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("QuantityCheckId");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("QuantityCheckId");

                    b.HasIndex("UserId");

                    b.ToTable("UserQuantityChecks");
                });

            modelBuilder.Entity("ApiServer.Model.UserTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ImageId");

                    b.Property<int?>("TagId");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("TagId");

                    b.HasIndex("UserId");

                    b.ToTable("UserTags");
                });

            modelBuilder.Entity("ApiServer.Model.UserTaggedTime", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ImageId");

                    b.Property<double>("TaggedTime");

                    b.Property<long?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("UserId");

                    b.ToTable("userTaggedTimes");
                });

            modelBuilder.Entity("VDS.BackgroundJobs.BackgroundJobInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<bool>("IsAbandoned");

                    b.Property<string>("JobArgs")
                        .IsRequired()
                        .HasMaxLength(1048576);

                    b.Property<string>("JobType")
                        .IsRequired()
                        .HasMaxLength(512);

                    b.Property<DateTimeOffset?>("LastTryTime");

                    b.Property<DateTimeOffset>("NextTryTime");

                    b.Property<byte>("Priority");

                    b.Property<short>("TryCount");

                    b.HasKey("Id");

                    b.ToTable("BackgroundJobs");
                });

            modelBuilder.Entity("VDS.IdentityServer4.PersistedGrantEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClientId");

                    b.Property<DateTime>("CreationTime");

                    b.Property<string>("Data");

                    b.Property<DateTime?>("Expiration");

                    b.Property<string>("SubjectId");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.ToTable("PersistedGrants");
                });

            modelBuilder.Entity("VDS.Messaging.Events.EventLogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<DateTime>("CreationTime");

                    b.Property<Guid>("EventId");

                    b.Property<string>("EventTypeName");

                    b.Property<int>("State");

                    b.Property<int>("TimesSend");

                    b.HasKey("Id");

                    b.ToTable("EventLogEntries");
                });

            modelBuilder.Entity("VDS.Notifications.NotificationInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("Data")
                        .HasMaxLength(1048576);

                    b.Property<string>("DataTypeName")
                        .HasMaxLength(512);

                    b.Property<string>("EntityId")
                        .HasMaxLength(96);

                    b.Property<string>("EntityTypeAssemblyQualifiedName")
                        .HasMaxLength(512);

                    b.Property<string>("EntityTypeName")
                        .HasMaxLength(250);

                    b.Property<string>("ExcludedUserIds")
                        .HasMaxLength(131072);

                    b.Property<string>("NotificationDataTypeAssemblyQualifiedName")
                        .IsRequired()
                        .HasMaxLength(512);

                    b.Property<string>("NotificationName")
                        .IsRequired()
                        .HasMaxLength(96);

                    b.Property<byte>("Severity");

                    b.Property<string>("UserIds")
                        .HasMaxLength(131072);

                    b.HasKey("Id");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("VDS.Notifications.NotificationSubscriptionInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<string>("EntityId")
                        .HasMaxLength(96);

                    b.Property<string>("EntityTypeAssemblyQualifiedName")
                        .HasMaxLength(512);

                    b.Property<string>("EntityTypeName")
                        .HasMaxLength(250);

                    b.Property<string>("NotificationName")
                        .HasMaxLength(96);

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("NotificationName", "EntityTypeName", "EntityId", "UserId");

                    b.ToTable("NotificationSubscriptions");
                });

            modelBuilder.Entity("VDS.Notifications.UserNotificationInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<Guid>("NotificationId");

                    b.Property<int>("State");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "State", "CreationTime");

                    b.ToTable("UserNotifications");
                });

            modelBuilder.Entity("VDS.Security.Permissions.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Category");

                    b.Property<string>("Description");

                    b.Property<string>("DisplayName");

                    b.Property<string>("Name");

                    b.Property<int?>("ParentId");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("ParentId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("VDS.Security.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<long?>("DeleterUserId");

                    b.Property<DateTime?>("DeletionTime");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastModificationTime");

                    b.Property<long?>("LastModifierUserId");

                    b.Property<string>("NormalizedRoleName");

                    b.Property<bool>("ProjectRole");

                    b.Property<string>("RoleName");

                    b.HasKey("Id");

                    b.HasIndex("CreatorUserId");

                    b.HasIndex("DeleterUserId");

                    b.HasIndex("LastModifierUserId");

                    b.HasIndex("NormalizedRoleName");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("VDS.Security.RoleClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("ClaimType");

                    b.HasIndex("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims");
                });

            modelBuilder.Entity("VDS.Security.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<bool>("EmailConfirmed");

                    b.Property<Guid?>("ImageId");

                    b.Property<Guid?>("ImageId1");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsLockoutEnabled");

                    b.Property<DateTime?>("LockoutEndDateUtc");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("NormalizedEmail");

                    b.Property<string>("NormalizedUserName");

                    b.Property<string>("PasswordHash")
                        .IsRequired();

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("Surname")
                        .IsRequired();

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("ImageId1");

                    b.HasIndex("NormalizedEmail");

                    b.HasIndex("NormalizedUserName");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("VDS.Security.UserLogin", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LoginProvider")
                        .IsRequired();

                    b.Property<string>("ProviderDisplayName")
                        .IsRequired();

                    b.Property<string>("ProviderKey")
                        .IsRequired();

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("LoginProvider", "ProviderKey");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("VDS.Security.UserRole", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<long?>("CreatorUserId");

                    b.Property<int>("RoleId");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("VDS.Settings.Setting", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Category");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("ApiServer.Model.Class", b =>
                {
                    b.HasOne("ApiServer.Model.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId");
                });

            modelBuilder.Entity("ApiServer.Model.Image", b =>
                {
                    b.HasOne("ApiServer.Model.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId");

                    b.HasOne("ApiServer.Model.QuantityCheck", "QuantityCheck")
                        .WithOne("Image")
                        .HasForeignKey("ApiServer.Model.Image", "QuantityCheckId");
                });

            modelBuilder.Entity("ApiServer.Model.PermissionRole", b =>
                {
                    b.HasOne("VDS.Security.Permissions.Permission", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VDS.Security.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ApiServer.Model.Project", b =>
                {
                    b.HasOne("ApiServer.Model.ProjectSetting", "ProjectSetting")
                        .WithOne("Project")
                        .HasForeignKey("ApiServer.Model.Project", "ProjectSettingId");
                });

            modelBuilder.Entity("ApiServer.Model.ProjectUser", b =>
                {
                    b.HasOne("ApiServer.Model.Project", "Project")
                        .WithMany("Users")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VDS.Security.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VDS.Security.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ApiServer.Model.Tag", b =>
                {
                    b.HasOne("ApiServer.Model.Class", "Class")
                        .WithMany("Tags")
                        .HasForeignKey("ClassId");

                    b.HasOne("ApiServer.Model.Image", "Image")
                        .WithMany("Tags")
                        .HasForeignKey("ImageId");

                    b.HasOne("ApiServer.Model.QuantityCheck", "QuantityCheck")
                        .WithMany()
                        .HasForeignKey("QuantityCheckId");
                });

            modelBuilder.Entity("ApiServer.Model.UserQuantityCheck", b =>
                {
                    b.HasOne("ApiServer.Model.QuantityCheck", "QuantityCheck")
                        .WithMany("UsersQc")
                        .HasForeignKey("QuantityCheckId");

                    b.HasOne("VDS.Security.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("ApiServer.Model.UserTag", b =>
                {
                    b.HasOne("ApiServer.Model.Image", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId");

                    b.HasOne("ApiServer.Model.Tag")
                        .WithMany("UsersTagged")
                        .HasForeignKey("TagId");

                    b.HasOne("VDS.Security.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("ApiServer.Model.UserTaggedTime", b =>
                {
                    b.HasOne("ApiServer.Model.Image", "Image")
                        .WithMany("UserTaggedTimes")
                        .HasForeignKey("ImageId");

                    b.HasOne("VDS.Security.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("VDS.Security.Permissions.Permission", b =>
                {
                    b.HasOne("VDS.Security.Permissions.Permission", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("VDS.Security.Role", b =>
                {
                    b.HasOne("VDS.Security.User", "CreatorUser")
                        .WithMany()
                        .HasForeignKey("CreatorUserId");

                    b.HasOne("VDS.Security.User", "DeleterUser")
                        .WithMany()
                        .HasForeignKey("DeleterUserId");

                    b.HasOne("VDS.Security.User", "LastModifierUser")
                        .WithMany()
                        .HasForeignKey("LastModifierUserId");
                });

            modelBuilder.Entity("VDS.Security.RoleClaim", b =>
                {
                    b.HasOne("VDS.Security.Role")
                        .WithMany("RoleClaims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VDS.Security.User", b =>
                {
                    b.HasOne("ApiServer.Model.Image")
                        .WithMany("UsersQc")
                        .HasForeignKey("ImageId");

                    b.HasOne("ApiServer.Model.Image")
                        .WithMany("UsersTagged")
                        .HasForeignKey("ImageId1");
                });

            modelBuilder.Entity("VDS.Security.UserLogin", b =>
                {
                    b.HasOne("VDS.Security.User")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VDS.Security.UserRole", b =>
                {
                    b.HasOne("VDS.Security.User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
