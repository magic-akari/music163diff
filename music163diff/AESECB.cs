using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace music163diff
{
    public static class AESECB
    {
        public static string NetEaseMusic163LinuxEncryptor(string origin)
        {
            var originBytes = Encoding.UTF8.GetBytes(origin);
            var keyStr = "7246674226682325323F5E6544673A51";
            var key = Enumerable.Range(0, keyStr.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(keyStr.Substring(x, 2), 16))
                .ToArray();

            RijndaelManaged aesAlg = new RijndaelManaged
            {
                Key = key,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform encryptor = aesAlg.CreateEncryptor();

            var result = encryptor.TransformFinalBlock(originBytes, 0, originBytes.Length);
            return BitConverter.ToString(result, 0, result.Length).Replace("-", "");
        }

    }
}
