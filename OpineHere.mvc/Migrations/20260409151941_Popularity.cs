using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace OpineHere.mvc.Migrations
{
    /// <inheritdoc />
    public partial class Popularity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MarkdownPost",
                type: "longtext",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "PopularityApproval",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    inUse = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PostId = table.Column<Guid>(type: "char(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopularityApproval", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PopularityApproval");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MarkdownPost");
        }
    }
}
