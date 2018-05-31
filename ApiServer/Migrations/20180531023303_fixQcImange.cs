using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ApiServer.Migrations
{
    public partial class fixQcImange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_QuantityChecks_QuantityCheckId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectSettings_ProjectSettingId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectSettingId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Images_QuantityCheckId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ProjectSettingId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QuantityCheckId",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "QuantityChecks",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ProjectSettings",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_QuantityChecks_ImageId",
                table: "QuantityChecks",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSettings_ProjectId",
                table: "ProjectSettings",
                column: "ProjectId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSettings_Projects_ProjectId",
                table: "ProjectSettings",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuantityChecks_Images_ImageId",
                table: "QuantityChecks",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSettings_Projects_ProjectId",
                table: "ProjectSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_QuantityChecks_Images_ImageId",
                table: "QuantityChecks");

            migrationBuilder.DropIndex(
                name: "IX_QuantityChecks_ImageId",
                table: "QuantityChecks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectSettings_ProjectId",
                table: "ProjectSettings");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectSettings");

            migrationBuilder.AddColumn<int>(
                name: "ProjectSettingId",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityCheckId",
                table: "Images",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectSettingId",
                table: "Projects",
                column: "ProjectSettingId",
                unique: true,
                filter: "[ProjectSettingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Images_QuantityCheckId",
                table: "Images",
                column: "QuantityCheckId",
                unique: true,
                filter: "[QuantityCheckId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_QuantityChecks_QuantityCheckId",
                table: "Images",
                column: "QuantityCheckId",
                principalTable: "QuantityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectSettings_ProjectSettingId",
                table: "Projects",
                column: "ProjectSettingId",
                principalTable: "ProjectSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
