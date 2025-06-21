using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZODs.Api.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_AcceptedByUser_Column_On_WorkspaceMemberInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcceptedByUserId",
                table: "WorkspaceMemberInvites",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMemberInvites_AcceptedByUserId",
                table: "WorkspaceMemberInvites",
                column: "AcceptedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMemberInvites_Users_AcceptedByUserId",
                table: "WorkspaceMemberInvites",
                column: "AcceptedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMemberInvites_Users_AcceptedByUserId",
                table: "WorkspaceMemberInvites");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMemberInvites_AcceptedByUserId",
                table: "WorkspaceMemberInvites");

            migrationBuilder.DropColumn(
                name: "AcceptedByUserId",
                table: "WorkspaceMemberInvites");
        }
    }
}
