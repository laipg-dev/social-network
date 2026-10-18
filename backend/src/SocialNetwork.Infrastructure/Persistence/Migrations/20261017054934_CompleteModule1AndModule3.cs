using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteModule1AndModule3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "UserProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MediaId",
                table: "PostMedia",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Theme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "light"),
                    NotificationEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "vi"),
                    AllowDirectMessagesFrom = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "EVERYONE"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_MediaId",
                table: "PostMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_UploaderId",
                table: "Media",
                column: "UploaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostMedia_Media_MediaId",
                table: "PostMedia",
                column: "MediaId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostMedia_Media_MediaId",
                table: "PostMedia");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_PostMedia_MediaId",
                table: "PostMedia");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "PostMedia");
        }
    }
}
