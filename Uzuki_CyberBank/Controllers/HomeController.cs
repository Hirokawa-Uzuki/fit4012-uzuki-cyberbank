using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Uzuki_CyberBank.Data;
using Uzuki_CyberBank.Models; 

namespace Uzuki_CyberBank.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class HomeController : Controller
    {
        private readonly UzukiDbContext _context;

        public HomeController(UzukiDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy ID người dùng đang đăng nhập từ Cookie
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Logout", "Auth");

            var userId = Guid.Parse(userIdClaim);

            // Lấy thông tin user hiện tại từ Database
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (currentUser == null) return RedirectToAction("Logout", "Auth");

            // Lấy danh sách các màn chơi
            var levels = await _context.GameLevels.OrderBy(l => l.LevelId).ToListAsync();

            // Truyền dữ liệu sang giao diện
            ViewBag.Levels = levels;

            return View(currentUser);
        }

        // Chức năng phụ: Hiển thị bảng xếp hạng (Leaderboard)
        public async Task<IActionResult> Leaderboard()
        {
            var topUsers = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.Score)
                .Take(10)
                .ToListAsync();

            return View(topUsers);
        }
    }
}