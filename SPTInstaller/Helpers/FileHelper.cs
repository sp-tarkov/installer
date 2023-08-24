using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

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
            Log.Error(ex, "Error while creating directories");
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
            Log.Error(ex, "Error while copying files");
            return Result.FromError(ex.Message);
        }
    }

    public static string GetRedactedPath(string path)
    {
        var nameMatched = Regex.Match(path, @".:\\[uU]sers\\(?<NAME>[^\\]+)");

        if (nameMatched.Success)
        {
            var name = nameMatched.Groups["NAME"].Value;
            return path.Replace(name, "-REDACTED-");
        }

        return path;
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
            Log.Error(ex, "Error during directory copy");
            return Result.FromError(ex.Message);
        }
    }

    public static void StreamAssemblyResourceOut(string resourceName, string outputFilePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        FileInfo outputFile = new FileInfo(outputFilePath);

        if (outputFile.Exists)
        {
            outputFile.Delete();
        }

        if (!outputFile.Directory.Exists)
        {
            Directory.CreateDirectory(outputFile.Directory.FullName);
        }

        var resName = assembly.GetManifestResourceNames().First(x => x.EndsWith(resourceName));

        using (FileStream fs = File.Create(outputFilePath))
        using (Stream s = assembly.GetManifestResourceStream(resName))
        {
            s.CopyTo(fs);
        }
    }

    public static bool CheckPathForProblemLocations(string path)
    {
        if (path.ToLower().EndsWith("desktop")) return true;

        var problemNames = new string[] {"onedrive", "nextcloud", "dropbox", "google" };

        return problemNames.Where(x => path.ToLower().Contains(x)).Count() > 0;
    }

}