using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class modifyTagQc2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_UserQcId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_UserTaggedId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UserQcId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_UserTaggedId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UserQcId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UserTaggedId",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId1",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "userTaggedTimes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageId = table.Column<Guid>(nullable: true),
                    TaggedTime = table.Column<double>(nullable: false),
                    UserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userTaggedTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userTaggedTimes_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_userTaggedTimes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImageId",
                table: "Users",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ImageId1",
                table: "Users",
                column: "ImageId1");

            migrationBuilder.CreateIndex(
                name: "IX_userTaggedTimes_ImageId",
                table: "userTaggedTimes",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_userTaggedTimes_UserId",
                table: "userTaggedTimes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Images_ImageId",
                table: "Users",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Images_ImageId1",
                table: "Users",
                column: "ImageId1",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Images_ImageId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Images_ImageId1",
                table: "Users");

            migrationBuilder.DropTable(
                name: "userTaggedTimes");

            migrationBuilder.DropIndex(
                name: "IX_Users_ImageId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ImageId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageId1",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "UserQcId",
                table: "Images",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserTaggedId",
                table: "Images",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_UserQcId",
                table: "Images",
                column: "UserQcId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_UserTaggedId",
                table: "Images",
                column: "UserTaggedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_UserQcId",
                table: "Images",
                column: "UserQcId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_UserTaggedId",
                table: "Images",
                column: "UserTaggedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
