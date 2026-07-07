# 🛡️ UZUKI CYBERBANK SECURITY SIMULATOR (FIT4012)

Chào mừng đến với **CyberBank Security Game v2** - Đồ án Trình giả lập An ninh mạng tương tác thời gian thực. Dự án thuộc học phần FIT4012 - Nhập môn An toàn bảo mật thông tin (Năm học 2025-2026), Trường Đại học Đại Nam.

### 👥 Đội ngũ Phát triển (Nhóm 15)
* **Nguyễn Ngọc Quyết (Leader)** - Kiến trúc Mật mã & Backend[cite: 4]
* **Phạm Đăng Quốc Dũng** - Thiết kế Giao thức & Frontend[cite: 4]
* **Lê Vũ Đình** - Kịch bản Bảo mật & QA Tester[cite: 4]

---

## 🎯 Mục tiêu Dự án
Dự án được xây dựng theo định hướng **Secure System Upgrade Challenge**, không chỉ cài đặt các thuật toán mật mã khô khan mà trực quan hóa chúng thông qua phong cách Gamification (Game hóa)[cite: 3]. Người dùng sẽ đóng vai Hacker thực hiện các cuộc tấn công mạng để kiểm chứng sức mạnh của kiến trúc phòng thủ đa tầng (Defense in Depth) do Nhóm 15 thiết kế.

## ⚔️ 7 Cấp độ Tấn công & Phòng thủ (Features)
Hệ thống tích hợp 7 chốt chặn bảo mật tương ứng với 7 kịch bản đe dọa (Threat Models) thực tế:
1. **Level 1 (Sniffing):** Chống nghe lén đường truyền bằng mã hóa **AES-256-GCM**.
2. **Level 2 (Tampering):** Đánh chặn hành vi sửa đổi gói tin (lật bit) thông qua thẻ xác thực **MAC Tag**.
3. **Level 3 (Replay Attack):** Ngăn chặn dội bom giao dịch cũ bằng cơ chế **Nonce Tracking** độc bản.
4. **Level 4 (Forgery):** Chống mạo danh Giám đốc nhờ sức mạnh của Chữ ký số **RSA-PSS 2048-bit**.
5. **Level 5 (Key Leak):** Loại bỏ rủi ro lộ khóa tĩnh bằng cơ chế **Xoay vòng khóa động (Session Key CSPRNG)**.
6. **Level 6 (SQL Injection):** Vô trùng hóa chuỗi truy vấn bằng cơ chế **LINQ Parameterization** của EF Core.
7. **Level 7 (Cryptographic DDoS):** Bảo vệ tài nguyên CPU trước Botnet thông qua lá chắn **Rate Limiting Middleware**.

## 🛠️ Công nghệ Sử dụng (Tech Stack)
* **Framework:** ASP.NET Core MVC (.NET 8).
* **Database:** SQL Server & Entity Framework Core (ORM).
* **Cryptography:** `System.Security.Cryptography` (AesGcm, RSACryptoServiceProvider, SHA256).
* **Frontend:** HTML5, CSS3 (Cyberpunk UI), JavaScript (AJAX Asynchronous).

## 🚀 Hướng dẫn Cài đặt
1. Clone Repository:
   `git clone https://github.com/Hirokawa-Uzuki/fit4012-uzuki-cyberbank.git`
2. Mở file `Uzuki_CyberBank.sln` bằng Visual Studio 2022.
3. Mở **Package Manager Console** và thực thi lệnh để khởi tạo CSDL:
   `Update-Database`
4. Nhấn **F5** để biên dịch và khởi chạy máy chủ mô phỏng.

## 🎥 Video Demo & Báo cáo
* **Báo cáo kỹ thuật:** Xem file PDF tại thư mục `/docs/`.
* **Video Demo:** [Chèn link YouTube của nhóm vào đây]

---
*Dự án được xây dựng hoàn toàn từ đầu (from scratch), không sử dụng lại mã nguồn cũ, cam kết tuân thủ các quy chuẩn khắt khe nhất về An toàn thông tin.*
