using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApiServer.Migrations
{
    public partial class addCommentQc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommentLevel1",
                table: "QuantityChecks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentLevel2",
                table: "QuantityChecks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentLevel3",
                table: "QuantityChecks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentLevel4",
                table: "QuantityChecks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommentLevel5",
                table: "QuantityChecks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentLevel1",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "CommentLevel2",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "CommentLevel3",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "CommentLevel4",
                table: "QuantityChecks");

            migrationBuilder.DropColumn(
                name: "CommentLevel5",
                table: "QuantityChecks");
        }
    }
}
