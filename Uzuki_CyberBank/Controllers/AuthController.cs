using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uzuki_CyberBank.Data;
using Uzuki_CyberBank.Models;
using Uzuki_CyberBank.ViewModels;

namespace Uzuki_CyberBank.Controllers
{
    public class AuthController : Controller
    {
        private readonly UzukiDbContext _context;

        public AuthController(UzukiDbContext context)
        {
            _context = context;
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem user đã tồn tại chưa
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Băm mật khẩu bằng BCrypt (Bao gồm cả thuật toán tạo Salt tự động)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword,
                    Score = 0,
                    CurrentLevel = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);

                // Ghi log (Yêu cầu bắt buộc của đề tài)
                _context.AuditLogs.Add(new AuditLog
                {
                    ActionType = "REGISTER",
                    LogDetails = $"Tài khoản {user.Username} được tạo mới.",
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return RedirectToAction("Login", "Auth");
            }
            return View(model);
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

                // Kiểm tra user có tồn tại và mật khẩu có khớp (hàm Verify tự lấy salt từ chuỗi hash để so sánh)
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    // Tạo phiên đăng nhập an toàn bằng Cookie
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // Ghi log đăng nhập thành công
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "LOGIN_SUCCESS",
                        LogDetails = $"Người dùng {user.Username} đăng nhập thành công.",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home"); // Chuyển hướng về Dashboard Game
                }

                // Ghi log đăng nhập thất bại
                _context.AuditLogs.Add(new AuditLog
                {
                    ActionType = "LOGIN_FAILED",
                    LogDetails = $"Đăng nhập thất bại với username: {model.Username}.",
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}