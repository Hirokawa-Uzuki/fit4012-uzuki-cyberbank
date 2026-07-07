using System;
using System.Security.Cryptography;
using System.Text;

namespace Uzuki_CyberBank.Services
{
    public class CryptoService : ICryptoService
    {
        #region AES-GCM (Confidentiality & Integrity)

        public string GenerateAesKey()
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        public (string CipherText, string Nonce, string Tag) EncryptTransaction(string payload, string base64Key)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(payload);

            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aesGcm = new AesGcm(key, tag.Length))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            return (
                Convert.ToBase64String(ciphertext),
                Convert.ToBase64String(nonce),
                Convert.ToBase64String(tag)
            );
        }

        public string DecryptTransaction(string cipherText, string base64Key, string nonce, string tag)
        {
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] ciphertextBytes = Convert.FromBase64String(cipherText);
            byte[] nonceBytes = Convert.FromBase64String(nonce);
            byte[] tagBytes = Convert.FromBase64String(tag);

            byte[] plaintextBytes = new byte[ciphertextBytes.Length];

            using (var aesGcm = new AesGcm(key, tagBytes.Length))
            {
                aesGcm.Decrypt(nonceBytes, ciphertextBytes, tagBytes, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        #endregion

        #region RSA-PSS (Authentication & Non-repudiation)

        public (string PublicKey, string PrivateKey) GenerateRsaKeyPair()
        {
            using (var rsa = RSA.Create(2048))
            {
                string privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
                string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
                return (publicKey, privateKey);
            }
        }

        public string SignData(string data, string privateKeyBase64)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

            using (var rsa = RSA.Create())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                return Convert.ToBase64String(signature);
            }
        }

        public bool VerifySignature(string data, string signatureBase64, string publicKeyBase64)
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = Convert.FromBase64String(signatureBase64);
                byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);

                using (var rsa = RSA.Create())
                {
                    rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                    return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}