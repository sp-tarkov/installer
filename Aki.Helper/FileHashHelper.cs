using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class FileHashHelper
    {
        public static bool CheckHash(FileInfo file, string expectedHash)
        {
            using (MD5 md5Service = MD5.Create())
            using (var sourceStream = file.OpenRead())
            {
                byte[] sourceHash = md5Service.ComputeHash(sourceStream);
                byte[] expectedHashBytes = Convert.FromBase64String(expectedHash);

                bool matched = Enumerable.SequenceEqual(sourceHash, expectedHashBytes);

                return matched;
            }
        }
    }
}
