using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class modifyRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Classes_ClassId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Images_ImageId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_QuantityChecks_QuantityChecksId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_QuantityChecksId",
                table: "Tags",
                newName: "IX_Tags_QuantityChecksId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_ImageId",
                table: "Tags",
                newName: "IX_Tags_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_ClassId",
                table: "Tags",
                newName: "IX_Tags_ClassId");

            migrationBuilder.AddColumn<bool>(
                name: "ProjectRole",
                table: "Roles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "ProjectUsers",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsers_RoleId",
                table: "ProjectUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectUsers_Roles_RoleId",
                table: "ProjectUsers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Classes_ClassId",
                table: "Tags",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Images_ImageId",
                table: "Tags",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_QuantityChecks_QuantityChecksId",
                table: "Tags",
                column: "QuantityChecksId",
                principalTable: "QuantityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectUsers_Roles_RoleId",
                table: "ProjectUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Classes_ClassId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Images_ImageId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_QuantityChecks_QuantityChecksId",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_ProjectUsers_RoleId",
                table: "ProjectUsers");

            migrationBuilder.DropColumn(
                name: "ProjectRole",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "ProjectUsers");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_QuantityChecksId",
                table: "Tag",
                newName: "IX_Tag_QuantityChecksId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_ImageId",
                table: "Tag",
                newName: "IX_Tag_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_ClassId",
                table: "Tag",
                newName: "IX_Tag_ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Classes_ClassId",
                table: "Tag",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Images_ImageId",
                table: "Tag",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_QuantityChecks_QuantityChecksId",
                table: "Tag",
                column: "QuantityChecksId",
                principalTable: "QuantityChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
