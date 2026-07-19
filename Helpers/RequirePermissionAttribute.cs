using System.Security.Claims;
using AHUWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Helpers
{
    // Lab 08: kiem tra ma quyen (permission code) cua tai khoan dang dang nhap, doc lap voi
    // Cookie Authentication dang dung that (chi la 1 lop kiem soat chi tiet hon BEN TRONG
    // khu vuc Admin, khong thay the [Authorize(Roles="admin")] dang bao ve toan bo khu vuc).
    //
    // Truy van CSDL truc tiep o moi request (khong cache vao Session) de dam bao khi admin
    // doi Nhom quyen cua 1 tai khoan thi co hieu luc NGAY, kg can doi tai khoan do dang xuat/
    // dang nhap lai. Neu tai khoan chua duoc gan vao Group nao thi coi nhu khong bi gioi han -
    // dam bao tai khoan admin mac dinh dang dung khong bi anh huong.

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public const string PermissionsSessionKey = "PermissionCodes";

        private readonly string _code;

        public RequirePermissionAttribute(string code)
        {
            _code = code;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated) return; // [Authorize] da xu ly truong hop nay

            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId)) return;

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var groupId = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.GroupId)
                .FirstOrDefaultAsync();

            if (!groupId.HasValue) return; // chua gan Group -> khong gioi han

            var hasCode = await db.GroupPermissions
                .AnyAsync(gp => gp.GroupId == groupId.Value && gp.Permission!.Code == _code);

            if (!hasCode)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
