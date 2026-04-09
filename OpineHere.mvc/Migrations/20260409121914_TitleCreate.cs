using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpineHere.mvc.Migrations
{
    /// <inheritdoc />
    public partial class TitleCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MarkdownPost",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "MarkdownPost");
        }
    }
}
