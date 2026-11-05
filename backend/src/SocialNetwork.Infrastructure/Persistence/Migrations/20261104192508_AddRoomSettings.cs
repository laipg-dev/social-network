using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_CommunityId",
                table: "Rooms");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Rooms",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Rooms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CommunityId_Name",
                table: "Rooms",
                columns: new[] { "CommunityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CommunityId_Position",
                table: "Rooms",
                columns: new[] { "CommunityId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_CommunityId_Name",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_CommunityId_Position",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CommunityId",
                table: "Rooms",
                column: "CommunityId");
        }
    }
}
