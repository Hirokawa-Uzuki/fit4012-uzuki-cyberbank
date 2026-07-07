using System;

namespace Uzuki_CyberBank.ViewModels
{
    public class Level1ViewModel
    {
        public Guid? TransactionId { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public decimal Amount { get; set; }

        // Dữ liệu mã hóa
        public string? RawJsonPayload { get; set; }
        public string? SecretKey { get; set; }
        public string? CipherText { get; set; }

        // Trạng thái game
        public bool IsCompleted { get; set; }
        public string? SystemMessage { get; set; }
    }

    public class Level2ViewModel
    {
        public string? SecretKey { get; set; }
        public string? Nonce { get; set; }
        public string? Tag { get; set; }

        // Dữ liệu mô phỏng bị tấn công
        public string? OriginalCipherText { get; set; }
        public string? TamperedCipherText { get; set; }

        // Trạng thái màn chơi
        public bool IsCompleted { get; set; }
        public bool HackDetected { get; set; } // True nếu hệ thống bắt được lỗi
        public string? SystemMessage { get; set; }
    }

    public class Level3ViewModel
    {
        public string? SecretKey { get; set; }
        public string? ValidCipherText { get; set; }
        public string? Nonce { get; set; }
        public string? Tag { get; set; }

        // Trạng thái màn chơi
        public bool IsCompleted { get; set; }
        public bool HackDetected { get; set; }
        public string? SystemMessage { get; set; }
    }

    public class Level4ViewModel
    {
        // Khóa bất đối xứng RSA
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; } // Khóa này Giám đốc giữ bí mật tuyệt đối

        // Giao dịch hợp lệ (Bị Hacker vứt bỏ)
        public string? ValidPayload { get; set; }
        public string? ValidSignature { get; set; }

        // Giao dịch giả mạo (Hacker gửi tới Server)
        public string? ForgedPayload { get; set; }
        public string? ForgedSignature { get; set; } // Chữ ký rác do Hacker tựaa bịa ra

        // Trạng thái màn chơi
        public bool IsCompleted { get; set; }
        public bool HackDetected { get; set; }
        public string? SystemMessage { get; set; }
    }
}