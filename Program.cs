using AHUWeb.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- MVC ----
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AHUWeb.Services.ICartService, AHUWeb.Services.CartService>();

// ---- EF Core / SQL Server (Somee) ----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Session (server-side cart, replaces localStorage) ----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---- Cookie authentication (Admin + Customer roles) ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AHUWeb.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ---- Apply pending EF Core migrations, then seed default admin account (admin / Admin@123) ----
// Wrapped so a DB connection/migration failure logs a clear message to stdout
// (visible once stdoutLogEnabled="true" in web.config) instead of surfacing only
// as a generic HTTP 500.30 with no detail.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        DbInitializer.Seed(db);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "STARTUP FAILURE while migrating/seeding the database. " +
            "Check ConnectionStrings:DefaultConnection in appsettings.json against the Somee database credentials.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// NOTE: HTTPS redirection/HSTS removed — Somee's free tier does not provide an
// SSL certificate for the app subdomain, so forcing HTTPS here causes redirect
// failures rather than a working site. Re-enable if you upgrade to a plan with SSL.
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
