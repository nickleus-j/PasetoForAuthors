using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpineHere.mvc.Migrations
{
    /// <inheritdoc />
    public partial class AuthorProfilesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AuthorProfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "AuthorProfile",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuthorProfile");
        }
    }
}
