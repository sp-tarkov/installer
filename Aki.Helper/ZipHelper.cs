﻿using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using Spectre.Console;
using System.Linq;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class ZipHelper
    {
        /// <summary>
        /// will extract Zips in LZMA compression format, using Zips path
        /// to new path
        /// </summary>
        public static void Decompress(string ArchivePath, string OutputFolderPath)
        {
            AnsiConsole.Progress().Columns(
    new PercentageColumn(),
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new ElapsedTimeColumn()
            ).Start((ProgressContext context) =>
            {
                using var archive = ZipArchive.Open(ArchivePath);
                var entries = archive.Entries.Where(entry => !entry.IsDirectory);
                var task = context.AddTask("Extracting", true, entries.Count());

                foreach (var entry in entries)
                {
                    entry.WriteToDirectory($"{OutputFolderPath}", new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });

                    task.Increment(1);
                }
            });
        }
    }
}
