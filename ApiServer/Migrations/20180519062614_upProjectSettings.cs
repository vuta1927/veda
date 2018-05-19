using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class upProjectSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectSettingId",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuantityCheckLevel = table.Column<int>(nullable: false),
                    TaggTimeValue = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectSettingId",
                table: "Projects",
                column: "ProjectSettingId",
                unique: true,
                filter: "[ProjectSettingId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectSettings_ProjectSettingId",
                table: "Projects",
                column: "ProjectSettingId",
                principalTable: "ProjectSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectSettings_ProjectSettingId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectSettings");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectSettingId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectSettingId",
                table: "Projects");
        }
    }
}
