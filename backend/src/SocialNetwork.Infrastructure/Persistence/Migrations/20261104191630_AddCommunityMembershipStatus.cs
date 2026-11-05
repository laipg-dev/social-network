using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityMembershipStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "CommunityRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CommunityMembers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "CommunityRoles" SET "IsDefault" = true
                WHERE "Name" = 'Member';

                UPDATE "CommunityMembers" SET "RequestedAt" = "JoinedAt"
                WHERE "RequestedAt" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_Status",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommunityMembers_CommunityId_Status",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "CommunityRoles");

            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CommunityMembers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "CommunityMembers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
