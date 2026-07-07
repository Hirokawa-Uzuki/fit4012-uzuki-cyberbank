using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Uzuki_CyberBank.Data;
using Uzuki_CyberBank.Models;
using Uzuki_CyberBank.Services;
using Uzuki_CyberBank.ViewModels;

namespace Uzuki_CyberBank.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly UzukiDbContext _context;
        private readonly ICryptoService _cryptoService;

        public GameController(UzukiDbContext context, ICryptoService cryptoService)
        {
            _context = context;
            _cryptoService = cryptoService;
        }

        // --- ĐIỀU HƯỚNG MÀN CHƠI ---
        public IActionResult PlayLevel(int levelId)
        {
            switch (levelId)
            {
                case 1: return RedirectToAction("Level1");
                case 2: return RedirectToAction("Level2");
                case 3: return RedirectToAction("Level3");
                case 4: return RedirectToAction("Level4");
                case 5: return RedirectToAction("Level5"); 
                case 6: return RedirectToAction("Level6"); 
                case 7: return RedirectToAction("Level7");
                default: return RedirectToAction("Index", "Home");
            }
        }

        // --- LEVEL 1: MÃ HÓA AES-GCM ---
        [HttpGet]
        public async Task<IActionResult> Level1()
        {
            // Lấy 2 tài khoản mẫu từ DB để làm giao dịch
            var accounts = await _context.BankAccounts.Take(2).ToListAsync();
            if (accounts.Count < 2) return BadRequest("Lỗi dữ liệu hệ thống!");

            var sender = accounts[0];
            var receiver = accounts[1];

            // Tạo gói tin giả lập (Plaintext)
            var payloadObj = new
            {
                From = sender.AccountNumber,
                To = receiver.AccountNumber,
                Amount = 50000
            };
            string jsonPayload = JsonSerializer.Serialize(payloadObj);

            var model = new Level1ViewModel
            {
                SenderName = sender.AccountName,
                ReceiverName = receiver.AccountName,
                Amount = 50000,
                RawJsonPayload = jsonPayload,
                SecretKey = _cryptoService.GenerateAesKey(), // Sinh key ngẫu nhiên cho màn chơi
                IsCompleted = false
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteLevel1(string rawJson, string secretKey)
        {
            try
            {
                // 1. Mã hóa gói tin
                var (cipherText, nonce, tag) = _cryptoService.EncryptTransaction(rawJson, secretKey);

                // 2. Lấy thông tin User hiện tại
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                // 3. Cộng điểm và qua màn (Nếu user đang ở Level 1)
                string sysMsg = "Mã hóa thành công!";
                if (user.CurrentLevel == 1)
                {
                    user.Score += 100; // Thưởng 100 điểm
                    user.CurrentLevel = 2; // Mở khóa Level 2
                    sysMsg = "Xuất sắc! Bạn đã bảo vệ thành công giao dịch và nhận được 100 KP.";

                    // Ghi log qua màn
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "DEFEND_SUCCESS",
                        LogDetails = $"Hoàn thành Level 1. Gói tin được mã hóa AES-GCM với CipherText: {cipherText.Substring(0, 20)}...",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                // 4. Trả kết quả về cho View
                var model = new Level1ViewModel
                {
                    RawJsonPayload = rawJson,
                    SecretKey = secretKey,
                    CipherText = cipherText, // Hiển thị chuỗi đã mã hóa
                    IsCompleted = true,
                    SystemMessage = sysMsg
                };

                return View("Level1", model);
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra trong quá trình mã hóa: " + ex.Message);
            }
        }

        // ========================================================================
        // LEVEL 2: KIỂM TRA TOÀN VẸN (INTERACTIVE MITM SIMULATOR)
        // ========================================================================
        [HttpGet]
        public IActionResult Level2()
        {
            // 1. Tạo gói tin gốc sạch sẽ (Giao dịch chuyển 100$ từ Giám đốc)
            string jsonPayload = "{\"From\":\"TK_GiamDoc\",\"To\":\"TK_DoiTac\",\"Amount\":100}";
            string secretKey = _cryptoService.GenerateAesKey();
            var (cipherText, nonce, tag) = _cryptoService.EncryptTransaction(jsonPayload, secretKey);

            // Đẩy gói tin nguyên bản sang View, cho phép người dùng tự tay "lật bit" phá hoại
            var model = new Level2ViewModel
            {
                SecretKey = secretKey,
                Nonce = nonce,
                Tag = tag,
                OriginalCipherText = cipherText,
                TamperedCipherText = cipherText, // Ban đầu chuỗi này nguyên vẹn
                IsCompleted = false
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FireAjaxLevel2(string tamperedCipherText, string secretKey, string nonce, string tag)
        {
            try
            {
                // Server tiến hành giải mã gói tin nhận được từ mạng
                string decryptedText = _cryptoService.DecryptTransaction(tamperedCipherText, secretKey, nonce, tag);

                // KỊCH BẢN 1: Nếu giải mã THÀNH CÔNG (Người dùng giữ nguyên gói tin, không phá hoại)
                return Json(new
                {
                    status = "normal",
                    code = 200,
                    message = $"[OK] Giao dịch hợp lệ! Server giải mã thành công luồng JSON: {decryptedText}"
                });
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                // KỊCH BẢN 2: BẮT ĐƯỢC LỖI SAI LỆCH TAG -> PHÒNG THỦ THÀNH CÔNG!
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (user.CurrentLevel == 2)
                {
                    user.Score += 150;
                    user.CurrentLevel = 3; // Mở khóa đường tiến vào Level 3

                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "DEFEND_SUCCESS",
                        LogDetails = "Level 2 Cleared: Phát hiện và ngăn chặn hành vi Data Tampering trên đường truyền bằng xác thực AES-GCM MAC Tag.",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    status = "tampered",
                    code = 400,
                    exceptionType = "System.Security.Cryptography.CryptographicException",
                    exceptionMessage = "The computed authentication tag did not match the input authentication tag.",
                    message = "[CRITICAL] PHÁT HIỆN TẤN CÔNG! Mã xác thực (Tag) không khớp. Nội dung gói tin đã bị can thiệp và lật bit (Bit-flipping Attack) trên đường truyền!"
                });
            }
        }

        // ========================================================================
        // LEVEL 3: CHỐNG REPLAY ATTACK (INTERACTIVE AJAX)
        // ========================================================================
        [HttpGet]
        public async Task<IActionResult> Level3()
        {
            var accounts = await _context.BankAccounts.Take(2).ToListAsync();
            var sender = accounts[0];
            var receiver = accounts[1];

            // 1. Tạo gói tin hợp lệ (Giao dịch gốc: Chuyển 5000$)
            string jsonPayload = $"{{\"From\":\"{sender.AccountNumber}\",\"To\":\"{receiver.AccountNumber}\",\"Amount\":5000}}";
            string secretKey = _cryptoService.GenerateAesKey();
            var (cipherText, nonce, tag) = _cryptoService.EncryptTransaction(jsonPayload, secretKey);

            // 2. CHUẨN BỊ MÔI TRƯỜNG CHO SIMULATOR
            // Dọn sạch các Nonce cũ trong Database để người chơi bắt đầu tự bấm gửi từ đầu
            var oldTxs = _context.SimulatedTransactions.Where(t => t.LevelId == 3);
            _context.SimulatedTransactions.RemoveRange(oldTxs);
            await _context.SaveChangesAsync();

            var model = new Level3ViewModel
            {
                SecretKey = secretKey,
                ValidCipherText = cipherText,
                Nonce = nonce,
                Tag = tag,
                IsCompleted = false
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FireAjaxReplay(string nonce)
        {
            // Kiểm tra xem Nonce đã có trong Database chưa?
            bool isReplay = await _context.SimulatedTransactions.AnyAsync(t => t.Nonce == nonce);

            if (!isReplay)
            {
                // [BỔ SUNG] Lấy 2 tài khoản mẫu để lấp đầy Khóa ngoại (Foreign Key)
                var accounts = await _context.BankAccounts.Take(2).ToListAsync();
                var sender = accounts[0];
                var receiver = accounts[1];

                // LẦN BẤM ĐẦU TIÊN: Nonce chưa từng xuất hiện -> Cho phép đi qua và LƯU LẠI
                var newTx = new SimulatedTransaction
                {
                    LevelId = 3,
                    SenderAccountId = sender.AccountId,     // THÊM DÒNG NÀY
                    ReceiverAccountId = receiver.AccountId, // THÊM DÒNG NÀY
                    OriginalAmount = 5000,
                    Nonce = nonce,
                    EncryptedPayload = "VALID_PAYLOAD_SIMULATION",
                    IsHacked = false,                       // THÊM DÒNG NÀY CHO CHUẨN DB
                    CreatedAt = DateTime.Now
                };

                _context.SimulatedTransactions.Add(newTx);
                await _context.SaveChangesAsync(); // Code sẽ chạy mượt mà qua đây!

                return Json(new
                {
                    status = "success",
                    code = 200,
                    message = $"[OK] Giao dịch 5.000$ thành công! Đã ghi nhận Nonce [{nonce.Substring(0, 8)}...] vào SQL Server."
                });
            }
            // ... (Phần else giữ nguyên)
            else
            {
                // TỪ LẦN BẤM THỨ 2 TRỞ ĐI: Nonce đã tồn tại -> Bắt quả tang Replay Attack!
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                // Cộng điểm thưởng nếu user chưa qua màn này
                if (user.CurrentLevel == 3)
                {
                    user.Score += 200;
                    user.CurrentLevel = 4;

                    // Có thể thêm AuditLog ở đây để hệ thống ghi vết
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "DEFEND_SUCCESS",
                        LogDetails = "Level 3 Cleared: Ngăn chặn Replay Attack bằng AJAX Simulator.",
                        CreatedAt = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    status = "blocked",
                    code = 403,
                    message = $"[FORBIDDEN] PHÁT HIỆN REPLAY ATTACK! Nonce [{nonce.Substring(0, 8)}...] đã tồn tại. Yêu cầu bị hủy!"
                });
            }
        }

        /// ========================================================================
        // LEVEL 4 (BOSS): THE CRYPTOGRAPHIC VAULT (RSA-PSS) - AJAX INTERACTIVE
        // ========================================================================
        [HttpGet]
        public async Task<IActionResult> Level4()
        {
            var accounts = await _context.BankAccounts.Take(2).ToListAsync();
            var ceo = accounts[0]; // Tài khoản Giám đốc
            var hacker = accounts[1]; // Tài khoản Hacker

            // 1. Khởi tạo Cặp khóa Bất đối xứng (RSA Key Pair) cho Giám đốc
            var (publicKey, privateKey) = _cryptoService.GenerateRsaKeyPair();

            // 2. Kịch bản Tấn công (Forgery Attack)
            // Hacker tự tạo một lệnh chuyển 1.000.000$ vào túi hắn
            string forgedPayload = $"{{\"From\":\"{ceo.AccountNumber}\",\"To\":\"{hacker.AccountNumber}\",\"Amount\":1000000}}";

            // Tạo sẵn một chuỗi rác để làm Fake Signature mẫu trên giao diện, 
            // cho phép người dùng xóa đi gõ lại tùy thích
            string defaultFakeSignature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("HACKER_FAKE_SIGNATURE_TRYING_TO_BYPASS_SECURITY_SYSTEM"));

            var model = new Level4ViewModel
            {
                PublicKey = publicKey,
                PrivateKey = privateKey,
                ValidPayload = "", // Không cần dùng cho giao diện mới
                ValidSignature = "", // Không cần dùng cho giao diện mới
                ForgedPayload = forgedPayload,
                ForgedSignature = defaultFakeSignature,
                IsCompleted = false
            };

            return View(model);
        }

        // 3. API XỬ LÝ AJAX KHI HACKER BẤM "KÍCH HOẠT QUÉT CHỮ KÝ"
        [HttpPost]
        public async Task<IActionResult> FireAjaxLevel4(string forgedPayload, string forgedSignature, string publicKey)
        {
            // Bức tường thép: Xác minh chữ ký bằng Public Key của Giám đốc
            bool isValidSignature = _cryptoService.VerifySignature(forgedPayload, forgedSignature, publicKey);

            if (!isValidSignature)
            {
                // PHÁT HIỆN MẠO DANH! Chữ ký không khớp với Public Key!
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (user.CurrentLevel == 4)
                {
                    user.Score += 300; // Thưởng đậm cho màn Boss
                    user.CurrentLevel = 5; // Mở đường tiến tới Level 5 (Rò rỉ khóa cứng)

                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "BOSS_DEFEATED",
                        LogDetails = "Level 4 Cleared: Ngăn chặn Forgery Attack bằng RSA-PSS. Bảo vệ thành công hệ thống cho Nhóm 15!",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                // Trả về JSON để Frontend làm hiệu ứng con dấu đỏ "TỪ CHỐI TRUY CẬP"
                return Json(new
                {
                    status = "blocked",
                    code = 403,
                    message = "[CRITICAL] XÁC THỰC THẤT BẠI! Chữ ký số không hợp lệ. Truy cập bị từ chối!"
                });
            }
            else
            {
                // Trường hợp này gần như bất khả thi trừ khi hacker đoán trúng 2048-bit Private Key
                return Json(new
                {
                    status = "success",
                    code = 200,
                    message = "THẢM HỌA: Hacker đã qua mặt hệ thống. 1.000.000$ đã bị đánh cắp!"
                });
            }
        }
        // ========================================================================
        // LEVEL 5: RÒ RỈ KHÓA CỨNG (HARDCODED KEY LEAK) & DYNAMIC ROTATION
        // ========================================================================
        [HttpGet]
        public IActionResult Level5()
        {
            // Truyền một "Khóa cứng" giả lập đã bị lộ lên Giao diện
            ViewBag.LeakedKey = "UZUKI_SECRET_KEY_2026_FIT4012_NHOM15";
            return View();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken] // Bức tường chống lỗi HTTP 400 do cơ chế Antiforgery Token của ASP.NET Core
        public async Task<IActionResult> FireAjaxLevel5()
        {
            try
            {
                // 1. Sinh khóa động mới
                string newDynamicKey = _cryptoService.GenerateAesKey();

                // 2. Lấy thông tin phiên đăng nhập
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { status = "error", code = 401, message = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return Json(new { status = "error", code = 404, message = "Không tìm thấy tài khoản người chơi trong CSDL!" });
                }

                // 3. Cập nhật tiến trình nếu tài khoản đang ở đúng Level 5
                // Nếu bạn đã đổi dữ liệu thủ công trong DB, đoạn này vẫn chạy qua và trả về khóa mới bình thường
                if (user.CurrentLevel == 5)
                {
                    user.Score += 200;
                    user.CurrentLevel = 6;

                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "DEFEND_SUCCESS",
                        LogDetails = "Level 5 Cleared: Kích hoạt Dynamic Key Rotation thành công.",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    status = "success",
                    code = 200,
                    newKey = newDynamicKey,
                    message = "[SYSTEM UPDATE] Khóa cứng đã bị hủy! Khóa động (Session Key) mới đã được cấp phát an toàn."
                });
            }
            catch (Exception ex)
            {
                // Bắt trọn gói nếu SQL Server bị ngắt kết nối hoặc lỗi cấu trúc bảng
                return Json(new { status = "error", code = 500, message = "Lỗi Server: " + ex.Message });
            }
        }

        // ========================================================================
        // LEVEL 6: TẤN CÔNG CẤU TRÚC DỮ LIỆU (SQL INJECTION)
        // ========================================================================
        [HttpGet]
        public IActionResult Level6()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FireAjaxLevel6(string maliciousInput)
        {
            // 1. KIỂM THỬ SỨC MẠNH CỦA ENTITY FRAMEWORK CORE
            // Hacker gửi lên chuỗi: ' OR '1'='1
            // Thay vì cộng chuỗi SQL thô sơ, EF Core sẽ tự động tham số hóa (Parameterization).
            // Lệnh dưới đây sẽ tìm người dùng có tên CHÍNH XÁC LÀ chuỗi "' OR '1'='1" (Chắc chắn = 0)

            var safeResults = await _context.BankAccounts
                                    .Where(b => b.AccountName == maliciousInput)
                                    .ToListAsync();

            // 2. Ghi nhận thành tích phòng thủ
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (user.CurrentLevel == 6)
            {
                user.Score += 250;
                user.CurrentLevel = 7; // Tiến thẳng vào Trận chiến DDoS cuối cùng

                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = user.UserId,
                    ActionType = "DEFEND_SUCCESS",
                    LogDetails = "Level 6 Cleared: Ngăn chặn hoàn toàn SQL Injection nhờ cơ chế Parameterization của EF Core.",
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            // 3. Trả về kết quả chứng minh Server vẫn an toàn
            return Json(new
            {
                status = "blocked",
                code = 200,
                resultsFound = safeResults.Count,
                message = $"[SHIELD ACTIVE] CSDL an toàn! Entity Framework đã bọc chuỗi [{maliciousInput}] thành văn bản vô hại (String Literal). Không có mã độc nào được thực thi!"
            });
        }

        // ========================================================================
        // LEVEL 7 (THE GRAND FINALE): CRYPTOGRAPHIC DDoS & RATE LIMITING
        // ========================================================================
        [HttpGet]
        public IActionResult Level7()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FireAjaxLevel7(bool isShieldActive)
        {
            // KỊCH BẢN 1: HỆ THỐNG PHÒNG THỦ CHƯA ĐƯỢC KÍCH HOẠT
            if (!isShieldActive)
            {
                // Mô phỏng Server bị "nghẽn cổ chai" vì phải giải mã RSA liên tục.
                // Hàm Delay này đại diện cho việc CPU đang bị vắt kiệt sức lực.
                await Task.Delay(1000);

                return Json(new
                {
                    status = "danger",
                    code = 200, // Mặc dù nguy hiểm nhưng Request vẫn lọt được vào Controller (HTTP 200)
                    message = "[CRITICAL WARNING] Máy chủ đang quá tải! CPU chạm ngưỡng 100% do phải tính toán giải mã hàng ngàn chữ ký số giả mạo. Nguy cơ sập hệ thống!"
                });
            }

            // KỊCH BẢN 2: LÁ CHẮN RATE LIMITING MIDDLEWARE ĐÃ BẬT
            else
            {
                // BỨC TƯỜNG THÉP: Request bị chặn đứng NGAY TẠI CỬA, 
                // không tốn dù chỉ 1 nhịp CPU nào để tính toán thuật toán mật mã.

                // Trào dâng vinh quang: Cập nhật điểm số Phá Đảo (Game Cleared)
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (user.CurrentLevel == 7)
                {
                    user.Score += 400; // Thưởng điểm tuyệt đối cho màn Boss cuối
                    user.CurrentLevel = 8; // Level 8 đại diện cho trạng thái Phá đảo hoàn toàn

                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = user.UserId,
                        ActionType = "GAME_CLEARED",
                        LogDetails = "Level 7 Cleared: Đánh bại đòn DDoS thuật toán bằng Rate Limiting. Nhóm 15 chính thức phá đảo hệ thống Uzuki_CyberBank!",
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                // Trả về mã lỗi 429 chuẩn chỉ của HTTP
                return Json(new
                {
                    status = "blocked",
                    code = 429,
                    message = "[HTTP 429 Too Many Requests] Request bị ném bỏ bởi Rate Limiter. Lõi Server được bảo vệ an toàn tuyệt đối!"
                });
            }
        }
    }
}