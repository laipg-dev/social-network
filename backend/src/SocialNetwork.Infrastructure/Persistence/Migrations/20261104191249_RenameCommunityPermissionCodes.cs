using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetwork.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCommunityPermissionCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chuẩn hoá mã permission sang UPPER_SNAKE theo module 2.
            // RolePermissions giữ nguyên vì vẫn trỏ tới cùng PermissionId.
            migrationBuilder.Sql(
                """
                UPDATE "Permissions" SET "Code" = 'COMMUNITY_EDIT'
                WHERE "Code" = 'community.update';

                UPDATE "Permissions" SET "Code" = 'COMMUNITY_DELETE'
                WHERE "Code" = 'community.delete';

                UPDATE "Permissions" SET "Code" = 'COMMUNITY_CLOSE'
                WHERE "Code" = 'community.close';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Permissions" SET "Code" = 'community.update'
                WHERE "Code" = 'COMMUNITY_EDIT';

                UPDATE "Permissions" SET "Code" = 'community.delete'
                WHERE "Code" = 'COMMUNITY_DELETE';

                UPDATE "Permissions" SET "Code" = 'community.close'
                WHERE "Code" = 'COMMUNITY_CLOSE';
                """);
        }
    }
}
