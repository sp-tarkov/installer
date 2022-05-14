using System;
using System.IO;
using Spectre.Console;
using System.Threading;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class FileHelper
    {
        public static int totalFiles;
        /// <summary>
        /// CopyDirectory will use old path and copy to new path and
        /// asks if inner files/folders should be included
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string oldDir, string newDir, bool recursive)
        {
            AnsiConsole.Progress().Columns(
            new TaskDescriptionColumn(),
            new SpinnerColumn(),
            new ElapsedTimeColumn()
            ).Start((ProgressContext context) =>
            {
                var dir = new DirectoryInfo(oldDir);

                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (FileInfo f in dir.GetFiles())
                {
                    totalFiles++;
                }
                foreach (DirectoryInfo subD in dirs)
                {
                    foreach (FileInfo f in subD.GetFiles())
                    {
                        totalFiles++;
                    }
                }

                var task = context.AddTask("Copying files: ", true, totalFiles);
                Directory.CreateDirectory(newDir);

                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(newDir, file.Name);
                    file.CopyTo(targetFilePath, true);
                }

                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(newDir, subDir.Name);
                        AltCopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            });
            
            //var dir = new DirectoryInfo(oldDir);

            //if (!dir.Exists)
            //    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            //DirectoryInfo[] dirs = dir.GetDirectories();

            //Directory.CreateDirectory(newDir);

            //foreach (FileInfo file in dir.GetFiles())
            //{
            //    string targetFilePath = Path.Combine(newDir, file.Name);
            //    file.CopyTo(targetFilePath, true);
            //}

            //if (recursive)
            //{
            //    foreach (DirectoryInfo subDir in dirs)
            //    {
            //        string newDestinationDir = Path.Combine(newDir, subDir.Name);
            //        CopyDirectory(subDir.FullName, newDestinationDir, true);
            //    }
            //}
        }

        public static void AltCopyDirectory(string oldDir, string newDir, bool recursive)
        {
            var dir = new DirectoryInfo(oldDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(newDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(newDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(newDir, subDir.Name);
                    AltCopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// DeleteFiles will use a type to look for, the path
        /// and if all inner files/folders should be included
        /// </summary>
        /// <remarks>
        /// Types are "file" or "folder" as a string
        /// </remarks>
        public static void DeleteFile(string type, string filePath, bool allFolders = false)
        {
            // type = "file" or "folder"
            if (string.Equals(type, "file", StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(filePath);
            }

            if (string.Equals(type, "folder", StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(filePath, allFolders);
            }
        }

        /// <summary>
        /// finds file based on Path and File name
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns>String or null</returns>
        public static string FindFile(string path, string name)
        {
            string[] filePaths = Directory.GetFiles(path);
            foreach (string file in filePaths)
            {
                if (file.Contains(name))
                {
                    return file;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds folder with name supplied, out = directory for extracted patch folder
        /// </summary>
        /// <param name="patchRef"></param>
        /// <param name="dir"></param>
        /// <returns>bool</returns>
        public static bool FindFolder(string patchRef, string targetPath, out DirectoryInfo dir)
        {
            var patchInfo = new FileInfo(patchRef);
            var patchName = patchInfo.Name.Replace(patchInfo.Extension, "");
            var path = new DirectoryInfo(Path.Join(targetPath, patchName));
            if (path.Exists)
            {
                dir = path;
                return true;
            }
            dir = null;
            return false;
        }
    }
}
