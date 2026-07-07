using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // BỔ SUNG: Thư viện Cookie Auth
using Uzuki_CyberBank.Data;
using Uzuki_CyberBank.Models;
using Uzuki_CyberBank.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký Database context có kèm cơ chế Retry On Failure
builder.Services.AddDbContext<UzukiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // THÊM DÒNG NÀY
    ));

// Đăng ký Crypto Service (Phần lõi bảo mật)
builder.Services.AddScoped<ICryptoService, CryptoService>();

// [BỔ SUNG QUAN TRỌNG]: Cấu hình Cookie Authentication cho hệ thống Đăng nhập
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Đường dẫn khi người dùng chưa đăng nhập
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Phiên tồn tại 60 phút
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// [BỔ SUNG QUAN TRỌNG]: Middleware Xác thực (Phải đứng TRƯỚC Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();