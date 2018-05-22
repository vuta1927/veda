using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class modifyTagQc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuantityChecks_Users_UserQcId",
                table: "QuantityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Users_UserTaggedId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserTaggedId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_QuantityChecks_UserQcId",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "UserTaggedId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UserQcId",
                table: "QuantityChecks");

            migrationBuilder.CreateTable(
                name: "UserQuantityChecks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuantityCheckId = table.Column<int>(nullable: true),
                    UserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuantityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuantityChecks_QuantityChecks_QuantityCheckId",
                        column: x => x.QuantityCheckId,
                        principalTable: "QuantityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserQuantityChecks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageId = table.Column<Guid>(nullable: true),
                    TagId = table.Column<int>(nullable: true),
                    UserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTags_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTags_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuantityChecks_QuantityCheckId",
                table: "UserQuantityChecks",
                column: "QuantityCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuantityChecks_UserId",
                table: "UserQuantityChecks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_ImageId",
                table: "UserTags",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_TagId",
                table: "UserTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_UserId",
                table: "UserTags",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQuantityChecks");

            migrationBuilder.DropTable(
                name: "UserTags");

            migrationBuilder.AddColumn<long>(
                name: "UserTaggedId",
                table: "Tags",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserQcId",
                table: "QuantityChecks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserTaggedId",
                table: "Tags",
                column: "UserTaggedId");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityChecks_UserQcId",
                table: "QuantityChecks",
                column: "UserQcId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuantityChecks_Users_UserQcId",
                table: "QuantityChecks",
                column: "UserQcId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Users_UserTaggedId",
                table: "Tags",
                column: "UserTaggedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
