using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;

namespace Installer.Aki.Helper
{
    public static class ZipHelper
    {
        /// <summary>
        /// will extract Zips in LZMA compression format, using Zips path 
        /// to new path
        /// </summary>
        public static void Decompress(string zipPath, string extPath)
        {
            Stream stream = File.OpenRead(zipPath);
            var reader = ReaderFactory.Open(stream);

            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    Console.WriteLine(reader.Entry.Key);
                    reader.WriteEntryToDirectory(extPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }
}
