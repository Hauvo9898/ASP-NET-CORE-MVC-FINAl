using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AHUWeb.Migrations
{
    /// <inheritdoc />
    public partial class Lab16_AddStaffFeedbackForumPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 3 ma quyen moi, tiep noi Lab09 (Group.Manage=1, Product.Manage=2, Article.Manage=3,
            // Order.Manage=4, User.Manage=5).
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 6, "Staff.Manage", "Quản lý nhân viên" },
                    { 7, "Feedback.Manage", "Quản lý liên hệ & góp ý" },
                    { 8, "Forum.Manage", "Quản lý diễn đàn" }
                });

            // Gan 3 quyen moi cho nhom "Toan quyen" (Id=1) de dung voi mo ta "duoc thao tac moi
            // chuc nang trong khu vuc quan tri". Cac nhom khac (vd Bien tap vien) khong duoc gan
            // mac dinh - admin co the vao /Admin/Groups de tick them neu muon.
            migrationBuilder.InsertData(
                table: "GroupPermissions",
                columns: new[] { "Id", "GroupId", "PermissionId" },
                values: new object[,]
                {
                    { 8, 1, 6 },
                    { 9, 1, 7 },
                    { 10, 1, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GroupPermissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "GroupPermissions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "GroupPermissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
