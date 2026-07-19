using System.Security.Claims;
using AHUWeb.Data;
using AHUWeb.Helpers;
using AHUWeb.Models;
using AHUWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHUWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    [RequirePermission("User.Manage")]
    public class UsersController : AdminBaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        // Form "Thêm hội viên" gốc không có ô mật khẩu (chỉ demo phía JS, không đăng nhập thật
        // được). Vì giờ có backend thật, user do admin tạo sẽ có mật khẩu mặc định này —
        // nên đổi ngay sau lần đăng nhập đầu.
        public const string DefaultNewUserPassword = "123456";

        public UsersController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /Admin/Users — thay cho renderAdminUsers()
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(u => u.Username.Contains(q) || (u.Email != null && u.Email.Contains(q)));

            ViewBag.CurrentQuery = q ?? "";
            return View(await query.OrderByDescending(u => u.Id).ToListAsync());
        }

        // Lab 08/09: danh sach Nhom quyen cho dropdown trong form Them/Sua
        private async Task LoadGroupsToViewBag()
        {
            ViewBag.Groups = await _db.Groups.OrderBy(g => g.Name)
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();
        }

        // GET /Admin/Users/Create — thay cho openUserModal() không id
        public async Task<IActionResult> Create()
        {
            await LoadGroupsToViewBag();
            return View(new UserFormViewModel { Role = "customer" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
                ModelState.AddModelError(nameof(model.Username), "Tên tài khoản đã tồn tại.");

            if (!ModelState.IsValid)
            {
                await LoadGroupsToViewBag();
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role,
                GroupId = model.GroupId,
                Password = BCrypt.Net.BCrypt.HashPassword(DefaultNewUserPassword)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = $"Đã tạo tài khoản. Mật khẩu mặc định: {DefaultNewUserPassword}";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Users/Edit/5 — thay cho openUserModal(userId)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new UserFormViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                GroupId = user.GroupId
            };
            await LoadGroupsToViewBag();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                await LoadGroupsToViewBag();
                return View(model);
            }

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.GroupId = model.GroupId;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã cập nhật người dùng";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Users/ToggleLock/5 — thay cho deleteUser()
        // Đổi từ xóa cứng sang khóa/mở khóa tài khoản (soft delete): không còn cần kiểm tra
        // "đã có đơn hàng" vì khóa không xóa dữ liệu, luôn an toàn với các Order liên quan.
        // Chặn admin tự khóa chính mình để tránh tự khóa mất quyền truy cập admin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id == currentUserId)
            {
                TempData["ToastMessage"] = "Không thể tự khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            user.IsLocked = !user.IsLocked;
            await _db.SaveChangesAsync();

            TempData["ToastMessage"] = user.IsLocked ? "Đã khóa tài khoản" : "Đã mở khóa tài khoản";
            return RedirectToAction(nameof(Index));
        }
    }
}
