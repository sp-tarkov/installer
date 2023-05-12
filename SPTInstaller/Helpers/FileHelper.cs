using ReactiveUI;
using SPTInstaller.Models;
using System;
using System.IO;

namespace SPTInstaller.Aki.Helper
{
    public static class FileHelper
    {
        private static Result IterateDirectories(DirectoryInfo sourceDir, DirectoryInfo targetDir)
        {
            try
            {
                foreach (var dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dir.FullName.Replace(sourceDir.FullName, targetDir.FullName));
                }
                return Result.FromSuccess();
            }
            catch (Exception ex) 
            {
                return Result.FromError(ex.Message);
            }
        }

        private static Result IterateFiles(DirectoryInfo sourceDir, DirectoryInfo targetDir, Action<string, int> updateCallback = null)
        {
            try
            {
                int totalFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories).Length;
                int processedFiles = 0;

                foreach (var file in sourceDir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    updateCallback?.Invoke(file.Name, (int)Math.Floor(((double)processedFiles / totalFiles) * 100));

                    File.Copy(file.FullName, file.FullName.Replace(sourceDir.FullName, targetDir.FullName), true);
                    processedFiles++;
                }

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(ex.Message);
            }
        }

        public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, IProgress<double> progress = null) =>
            CopyDirectoryWithProgress(sourceDir, targetDir, (msg, prog) => progress?.Report(prog));

        public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, Action<string, int> updateCallback = null)
        {
            try
            {
                var iterateDirectoriesResult = IterateDirectories(sourceDir, targetDir);

                if(!iterateDirectoriesResult.Succeeded) return iterateDirectoriesResult;

                var iterateFilesResult = IterateFiles(sourceDir, targetDir, updateCallback);

                if (!iterateFilesResult.Succeeded) return iterateDirectoriesResult;

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(ex.Message);
            }
        }
    }
}
