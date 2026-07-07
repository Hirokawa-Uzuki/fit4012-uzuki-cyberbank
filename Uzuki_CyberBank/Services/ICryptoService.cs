using System;

namespace Uzuki_CyberBank.Services
{
    public interface ICryptoService
    {
        // 1. Nhóm mã hóa đối xứng (Màn 1, 2, 3) - Sử dụng AES-GCM
        (string CipherText, string Nonce, string Tag) EncryptTransaction(string payload, string base64Key);
        string DecryptTransaction(string cipherText, string base64Key, string nonce, string tag);
        string GenerateAesKey();

        // 2. Nhóm chữ ký số bất đối xứng (Màn 4) - Sử dụng RSA-PSS
        (string PublicKey, string PrivateKey) GenerateRsaKeyPair();
        string SignData(string data, string privateKeyBase64);
        bool VerifySignature(string data, string signatureBase64, string publicKeyBase64);
    }
}