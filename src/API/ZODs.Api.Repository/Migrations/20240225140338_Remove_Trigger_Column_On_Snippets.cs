using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZODs.Api.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Trigger_Column_On_Snippets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Trigger",
                table: "Snippets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Trigger",
                table: "Snippets",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");
        }
    }
}
