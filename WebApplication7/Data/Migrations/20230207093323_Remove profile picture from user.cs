using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication7.Data.Migrations
{
    public partial class Removeprofilepicturefromuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                schema: "Identity",
                table: "User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                schema: "Identity",
                table: "User",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
