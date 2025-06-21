using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZODs.Api.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_RegistrationType_EnumColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistrationType",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationType",
                table: "Users");
        }
    }
}
