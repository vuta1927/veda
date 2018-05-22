using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class modifyTagQc3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuantityChecks_QuantityChecks_QuantityCheckId",
                table: "UserQuantityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuantityChecks_Users_UserId",
                table: "UserQuantityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_userTaggedTimes_Images_ImageId",
                table: "userTaggedTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_userTaggedTimes_Users_UserId",
                table: "userTaggedTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Images_ImageId",
                table: "UserTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags");

            migrationBuilder.DropIndex(
                name: "IX_UserTags_ImageId",
                table: "UserTags");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserTags",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TagId",
                table: "UserTags",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "UserTags",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "userTaggedTimes",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "userTaggedTimes",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserQuantityChecks",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "QuantityCheckId",
                table: "UserQuantityChecks",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuantityChecks_QuantityChecks_QuantityCheckId",
                table: "UserQuantityChecks",
                column: "QuantityCheckId",
                principalTable: "QuantityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuantityChecks_Users_UserId",
                table: "UserQuantityChecks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userTaggedTimes_Images_ImageId",
                table: "userTaggedTimes",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userTaggedTimes_Users_UserId",
                table: "userTaggedTimes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuantityChecks_QuantityChecks_QuantityCheckId",
                table: "UserQuantityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuantityChecks_Users_UserId",
                table: "UserQuantityChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_userTaggedTimes_Images_ImageId",
                table: "userTaggedTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_userTaggedTimes_Users_UserId",
                table: "userTaggedTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserTags",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "TagId",
                table: "UserTags",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "UserTags",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "userTaggedTimes",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "userTaggedTimes",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserQuantityChecks",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "QuantityCheckId",
                table: "UserQuantityChecks",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_ImageId",
                table: "UserTags",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuantityChecks_QuantityChecks_QuantityCheckId",
                table: "UserQuantityChecks",
                column: "QuantityCheckId",
                principalTable: "QuantityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuantityChecks_Users_UserId",
                table: "UserQuantityChecks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_userTaggedTimes_Images_ImageId",
                table: "userTaggedTimes",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_userTaggedTimes_Users_UserId",
                table: "userTaggedTimes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Images_ImageId",
                table: "UserTags",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Users_UserId",
                table: "UserTags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
