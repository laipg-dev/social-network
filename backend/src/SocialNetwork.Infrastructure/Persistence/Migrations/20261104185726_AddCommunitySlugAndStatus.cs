using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunitySlugAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm Slug dạng nullable trước để có thể backfill dữ liệu cũ,
            // sau đó mới siết NOT NULL + unique index.
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Communities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Communities",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.Sql(
                """
                UPDATE "Communities"
                SET "Slug" = COALESCE(
                    NULLIF(
                        trim(both '-' from regexp_replace(lower("Name"), '[^a-z0-9]+', '-', 'g')),
                        ''
                    ),
                    'community'
                ) || '-' || substring(replace("Id"::text, '-', '') from 1 for 8)
                WHERE "Slug" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Communities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_Slug",
                table: "Communities",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Communities_Slug",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Communities");
        }
    }
}
