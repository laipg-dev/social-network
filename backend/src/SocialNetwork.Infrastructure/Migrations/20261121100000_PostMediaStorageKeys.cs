using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SocialNetwork.Infrastructure.Persistence;

#nullable disable

namespace SocialNetwork.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261121100000_PostMediaStorageKeys")]
public sealed class PostMediaStorageKeys : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Preserve every pre-existing URL verbatim. These rows require a manual file migration
        // before they can receive a StorageKey; do not infer keys from URLs.
        migrationBuilder.RenameColumn(name: "Url", table: "PostMedia", newName: "LegacyUrl");
        migrationBuilder.AlterColumn<string>(name: "LegacyUrl", table: "PostMedia", type: "text", nullable: true, oldClrType: typeof(string), oldType: "text");
        migrationBuilder.AddColumn<string>(name: "StorageKey", table: "PostMedia", type: "character varying(256)", maxLength: 256, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep new identifiers recoverable in the old string column when reverting the schema.
        migrationBuilder.Sql("UPDATE \"PostMedia\" SET \"LegacyUrl\" = \"StorageKey\" WHERE \"StorageKey\" IS NOT NULL");
        migrationBuilder.DropColumn(name: "StorageKey", table: "PostMedia");
        migrationBuilder.AlterColumn<string>(name: "LegacyUrl", table: "PostMedia", type: "text", nullable: false, defaultValue: "", oldClrType: typeof(string), oldType: "text", oldNullable: true);
        migrationBuilder.RenameColumn(name: "LegacyUrl", table: "PostMedia", newName: "Url");
    }
}
