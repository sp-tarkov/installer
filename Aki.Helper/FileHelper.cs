using System;
using System.IO;
using Spectre.Console;
using SPT_AKI_Installer.Aki.Core.Model;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class FileHelper
    {
        public static GenericResult CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, IProgress<double> progress)
        {
            try
            {
                int totalFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories).Length;
                int processedFiles = 0;

                foreach (var dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dir.FullName.Replace(sourceDir.FullName, targetDir.FullName));
                }

                foreach (var file in sourceDir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    File.Copy(file.FullName, file.FullName.Replace(sourceDir.FullName, targetDir.FullName), true);
                    processedFiles++;

                    progress.Report((int)Math.Floor(((double)processedFiles / totalFiles) * 100));
                }

                return GenericResult.FromSuccess();
            }
            catch(Exception ex)
            {
                return GenericResult.FromError(ex.Message);
            }
        }

        public static void DeleteFiles(string filePath, bool allFolders = false)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                if (filePath.Contains('.'))
                {
                    File.Delete(filePath);
                }
                else
                {
                    Directory.Delete(filePath, allFolders);
                }
            }
        }

        public static string FindFile(string path, string name)
        {
            string[] filePaths = Directory.GetFiles(path);
            foreach (string file in filePaths)
            {
                if (file.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }

        public static string FindFile(string path, string name, string altName)
        {
            string[] filePaths = Directory.GetFiles(path);
            foreach (string file in filePaths)
            {
                if (file.Contains(name, StringComparison.OrdinalIgnoreCase) &&
                    file.Contains(altName, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }

        public static bool FindFolder(string patchRef, string targetPath, out DirectoryInfo dir)
        {
            var dirInfo = new DirectoryInfo(targetPath).GetDirectories();
            string patchInner = null;
            foreach (var file in dirInfo)
            {
                if (file.FullName.Contains("patcher", StringComparison.OrdinalIgnoreCase))
                {
                    patchInner = file.FullName;
                }
            }
            var path = new DirectoryInfo(patchInner);
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
