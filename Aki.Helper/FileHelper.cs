using System;
using System.IO;
using Spectre.Console;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class FileHelper
    {
        public static void CopyDirectory(string oldDir, string newDir, bool overwrite)
        {
            int totalFiles = Directory.GetFiles(oldDir, "*.*", SearchOption.AllDirectories).Length;

            AnsiConsole.Progress().Columns(
        new PercentageColumn(),
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new ElapsedTimeColumn(),
            new SpinnerColumn()
            ).Start((ProgressContext context) =>
            {
                var task = context.AddTask("Copying Files", true, totalFiles);

                foreach (string dirPath in Directory.GetDirectories(oldDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(oldDir, newDir));
                }

                foreach (string newPath in Directory.GetFiles(oldDir, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(oldDir, newDir), overwrite);
                    task.Increment(1);
                }
            });
        }

        public static void DeleteFiles(string filePath, bool allFolders = false)
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
