# Đối chiếu Lab 01 → 17 với source code trong repo

Project này (**AHUWeb**) được xây trên nền **ASP.NET Core MVC (.NET 8)**, không dùng nguyên
template gốc "management" của giáo trình mà style hóa lại theo thương hiệu thời trang AHU.
Vì vậy một số kỹ thuật được hiện thực **đúng bản chất nghiệp vụ nhưng khác công cụ cụ thể**
(ví dụ: Cookie Authentication + Claims thay cho Session thuần, form MVC postback thay cho
jQuery Ajax + Modal). Các chỗ khác biệt được ghi chú rõ bên dưới, không giấu.

---

## Lab 01 — Môi trường & CSDL Code-First
- **File:** `Models/*.cs` (Product, Article, User, Order, OrderDetail, Staff, Feedback, Category,
  Group, Permission...), `Data/ApplicationDbContext.cs`
- **Migration khởi tạo:** `Migrations/20260711025701_InitialCreate.cs`
- Khác giáo trình: dùng `int Id` (Identity) thay vì `Guid`, vì SQL Server trên Somee giới hạn
  dung lượng 30MB, `int` tiết kiệm hơn `Guid` đáng kể ở scale nhỏ.

## Lab 02 — Kết nối SQL Server & Area Admin
- **Kết nối DB:** `appsettings.json` → `ConnectionStrings:DefaultConnection`,
  cấu hình trong `Program.cs` (`AddDbContext<ApplicationDbContext>`)
- **Area Admin:** `Areas/Admin/` (toàn bộ Controllers, Views, Models con trong đó)
- **Route Area:** khai báo trong `Program.cs` (`MapControllerRoute` cho area "Admin")

## Lab 03 — Giao diện Admin & Site
- **Layout Admin:** `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- **Layout Site:** `Views/Shared/_Layout.cshtml`
- **Asset:** `wwwroot/css/style.css`, `wwwroot/js/`, `wwwroot/images/`

## Lab 04 — Danh sách dữ liệu (bảng động)
- **File:** `Areas/Admin/Views/Products/Index.cshtml`, `.../Orders/Index.cshtml`,
  `.../Users/Index.cshtml`... (mỗi Controller Admin đều có `Index(string? q)` hỗ trợ tìm kiếm)
- Khác giáo trình: dùng **Razor server-render + query string `?q=`** thay vì Ajax/JDataTable
  (đơn giản hơn, phù hợp scale nhỏ, không cần bảng dữ liệu phân trang phức tạp).

## Lab 05 — Thêm/Sửa/Xóa (CRUD)
- **File mẫu:** `Areas/Admin/Controllers/ProductsController.cs` (`Create`, `Edit`, `Delete`),
  view form: `Areas/Admin/Views/Products/_Form.cshtml`
- Khác giáo trình: dùng **trang riêng (`Create.cshtml`/`Edit.cshtml`) + form MVC postback**
  thay vì Modal Bootstrap + Ajax. Thông báo kết quả qua `TempData["ToastMessage"]`
  (tương đương vai trò của Toastr).

## Lab 06 — Upload ảnh & quản lý thành viên
- **Upload avatar:** `Controllers/AccountController.cs` → action `UploadAvatar(IFormFile avatarFile)`
- **Upload ảnh sản phẩm/bài viết:** `Areas/Admin/Controllers/ArticlesController.cs`,
  `ProductsController.cs`, `StaffsController.cs` (đều dùng `IFormFile`)
- **Mã hóa mật khẩu:** `BCrypt.Net.BCrypt.HashPassword(...)` trong `AccountController.cs`
  (khác giáo trình: dùng **BCrypt thay vì MD5** — an toàn hơn vì MD5 đã bị xem là không đủ
  an toàn cho mật khẩu từ lâu)
- **Ràng buộc xóa thành viên đã có dữ liệu liên quan:** ban đầu chặn xóa nếu User có Order
  (đúng tinh thần "đã viết bài thì không xóa" của giáo trình), sau đó **nâng cấp thành khóa/mở
  khóa tài khoản (soft delete)** — xem `Areas/Admin/Controllers/UsersController.cs` action
  `ToggleLock`, cột `IsLocked` trong `Models/User.cs`, migration
  `20260719050000_Lab17_AddUserIsLocked.cs`.

## Lab 07 — Đăng nhập
- **File:** `Controllers/AccountController.cs` → action `Login`
- Khác giáo trình: dùng **ASP.NET Core Cookie Authentication + Claims**
  (`HttpContext.SignInAsync(...)`) thay vì tự lưu object vào Session — đây là cách làm chuẩn
  và an toàn hơn của .NET hiện đại, thay thế hoàn toàn vai trò của Session login trong bài gốc.
- **Chặn truy cập khi chưa đăng nhập:** `[Authorize(Roles = "admin")]` trên các Controller Admin
  (tương đương Custom Action Filter của giáo trình).

## Lab 08 — Phân quyền theo chức năng
- **File:** `Helpers/RequirePermissionAttribute.cs` (attribute tự viết, gắn trên từng Controller
  Admin, ví dụ `[RequirePermission("User.Manage")]` trong `UsersController.cs`)
- **Gán quyền theo nhóm:** `Models/Group.cs`, `GroupId` trong `Models/User.cs`
  (migration `20260718123521_Lab08_AddUserGroupId.cs`)

## Lab 09 — Seed dữ liệu & giao diện phân quyền
- **Seed dữ liệu mặc định:** migration `20260718124437_Lab09_SeedGroupPermissionData.cs`
- **Giao diện gán quyền:** `Areas/Admin/Views/Groups/Index.cshtml`,
  `Areas/Admin/Controllers/GroupsController.cs`

## Lab 10 — Active menu & danh mục cha-con
- **Active nav:** `wwwroot/js/nav-active.js`
- **Danh mục cha-con:** `Models/Category.cs` (`ParentId`), hiển thị trong
  `Areas/Admin/Views/Categories/`

## Lab 11 — Quản lý sản phẩm
- **File:** `Areas/Admin/Controllers/ProductsController.cs`,
  `Areas/Admin/Views/Products/_Form.cshtml`
- **Audit fields (CreatedBy/ModifiedBy):** migration
  `20260718125602_Lab11_AddProductAuditFields.cs`
- **Summernote cho ô "Mô tả sản phẩm":** tích hợp trong `_Form.cshtml` (CDN jQuery +
  Summernote 0.8.20), callback `onImageUpload` gọi action
  `ProductsController.UploadImage` để lưu ảnh thật và chèn URL vào nội dung.

## Lab 12 — Quản lý tin tức & upload ảnh trong bài viết
- **File:** `Areas/Admin/Controllers/ArticlesController.cs` → action `UploadImage(IFormFile file)`
  (upload ảnh riêng, chỉ lưu đường dẫn — không lưu Base64 vào CSDL, đúng tinh thần tối ưu của bài)
- **Summernote cho ô "Nội dung" bài viết:** tích hợp trong
  `Areas/Admin/Views/Articles/Create.cshtml` và `Edit.cshtml`, dùng chung cơ chế
  `onImageUpload` như Lab 11.
- **Hiển thị nội dung HTML:** `Views/Article/Details.cshtml` dùng `@Html.Raw(Model.Content)`;
  `Helpers/FormatHelper.cs` có hàm `StripHtml` để tính đoạn trích (`Excerpt`) và thời gian
  đọc (`ReadingMinutes`) đúng trên chữ thật, không lẫn thẻ HTML do Summernote sinh ra.

## Lab 13 — Quản lý đơn hàng
- **CSDL:** `Models/Order.cs`, `Models/OrderDetail.cs`, `Models/User.cs` (đóng vai trò Customer)
- **Danh sách & chi tiết đơn:** `Areas/Admin/Controllers/OrdersController.cs`,
  `Areas/Admin/Views/Orders/Index.cshtml`, `Details.cshtml`
- **Thuế VAT 10% + phí vận chuyển:** `Helpers/` (helper tính tổng hóa đơn), áp dụng trong
  `Views/Order/Checkout.cshtml` và `Areas/Admin/Views/Orders/Details.cshtml`

## Lab 14 — Thống kê doanh thu (Chart.js)
- **File:** `Areas/Admin/Views/Dashboard/Reports.cshtml` (nhúng Chart.js CDN,
  hàm `drawMonthlyChart`)
- **Truy vấn GroupBy theo tháng:** `Areas/Admin/Controllers/DashboardController.cs`
  (`orders.GroupBy(o => o.Date.Month)...`)

## Lab 15 — Partial View tái sử dụng
- **File:** `Views/Product/_ProductCard.cshtml`, `_ProductGridSection.cshtml`
  (dùng lại ở cả Trang chủ và Trang sản phẩm)
- **Sản phẩm "Sắp ra mắt":** cột `IsComingSoon` trong `Models/Product.cs`,
  migration `20260718131009_Lab15_AddProductIsComingSoon.cs`

## Lab 16 — Publish lên Hosting
- **Hosting thực tế dùng:** Somee.com (không phải smarterasp.net như ví dụ giáo trình,
  nhưng cùng bản chất: shared hosting hỗ trợ ASP.NET Core qua IIS)
- **Cấu hình:** `web.config` (ASP.NET Core Module), connection string
  `appsettings.json` cần trỏ đúng thông tin SQL Somee (`TrustServerCertificate=True`
  bắt buộc với Somee)
- **Lưu ý bảo mật đã áp dụng thêm:** connection string thật KHÔNG commit lên GitHub,
  xem `.gitignore` + `appsettings.Local.json` (không có trong repo, chỉ tồn tại local)

## Lab 17 — Mở rộng (thử nghiệm React)
- **File:** `Views/Home/ReactDemo.cshtml` — trang thử nghiệm nhúng React vào Razor View
- Đây là phần mở rộng/tuỳ chọn theo đúng tinh thần "Optional" của Lab 17 gốc; phần lõi
  ứng dụng vẫn dùng Razor MVC server-render, không tách riêng thành SPA/Web API độc lập.

---

## Ghi chú chung
- Danh sách migration đầy đủ (đánh số theo Lab) nằm trong `Migrations/`, có thể chạy
  `dotnet ef migrations list` để xem thứ tự áp dụng.
- Các chức năng phát sinh thêm ngoài 17 Lab gốc (Forum, Feedback, Staff, khóa/mở khóa tài
  khoản...) được đánh dấu `Fix_` hoặc mô tả rõ trong tên migration/commit message, không
  gộp lẫn vào các Lab chính để tránh gây hiểu nhầm khi chấm bài.
