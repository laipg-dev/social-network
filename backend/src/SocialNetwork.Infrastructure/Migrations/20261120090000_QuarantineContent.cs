using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SocialNetwork.Infrastructure.Persistence;

#nullable disable

namespace SocialNetwork.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261120090000_QuarantineContent")]
public partial class QuarantineContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(name: "QuarantinedAt", table: "Posts", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "QuarantinedAt", table: "Comments", type: "timestamp with time zone", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "QuarantinedAt", table: "Posts");
        migrationBuilder.DropColumn(name: "QuarantinedAt", table: "Comments");
    }
}
