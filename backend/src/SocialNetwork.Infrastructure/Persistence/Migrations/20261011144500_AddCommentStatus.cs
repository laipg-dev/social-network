using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations;

public partial class AddCommentStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "Comments",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "ACTIVE");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Status", table: "Comments");
    }
}
