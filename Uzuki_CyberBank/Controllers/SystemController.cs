using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uzuki_CyberBank.Data;
using Uzuki_CyberBank.Models;
using Uzuki_CyberBank.ViewModels;

namespace Uzuki_CyberBank.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới được xem Log
    public class SystemController : Controller
    {
        private readonly UzukiDbContext _context;

        public SystemController(UzukiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs()
        {
            // Lấy 100 log mới nhất, join với bảng User để lấy tên người dùng
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)
                .Select(l => new AuditLogViewModel
                {
                    // Nếu Log thuộc về hệ thống (UserId = null) thì để tên là SYSTEM
                    Username = l.User != null ? l.User.Username : "SYSTEM_NODE",
                    ActionType = l.ActionType,
                    LogDetails = l.LogDetails,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return View(logs);
        }

        // Tạo sẵn các hàm trống để làm Threat Model và Server Status sau
        public IActionResult ThreatModel()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ServerStatus()
        {
            // Lấy dữ liệu thật từ Database để Dashboard trông "uy tín" hơn
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalTx = await _context.SimulatedTransactions.CountAsync();

            // Đếm số lần hệ thống phòng thủ thành công
            ViewBag.ThreatsBlocked = await _context.AuditLogs
                .CountAsync(l => l.ActionType.Contains("SUCCESS") || l.ActionType == "GAME_CLEARED");

            return View();
        }
    }
}