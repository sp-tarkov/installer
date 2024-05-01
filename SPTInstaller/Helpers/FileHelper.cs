using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class FileHelper
{
    private static Result IterateDirectories(DirectoryInfo sourceDir, DirectoryInfo targetDir, string[] exclusions)
    {
        try
        {
            foreach (var dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
            {
                var exclude = false;
                
                foreach (var exclusion in exclusions)
                {
                    var currentDirRelativePath = dir.FullName.Replace(sourceDir.FullName, "");
                    
                    if (currentDirRelativePath.StartsWith(exclusion) || currentDirRelativePath == exclusion)
                    {
                        exclude = true;
                        Log.Debug(
                            $"EXCLUSION FOUND :: DIR\nExclusion: '{exclusion}'\nPath: '{currentDirRelativePath}'");
                        break;
                    }
                }
                
                if (exclude)
                    continue;
                
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
    
    private static Result IterateFiles(DirectoryInfo sourceDir, DirectoryInfo targetDir, string[] exclusions,
        Action<string, int> updateCallback = null)
    {
        try
        {
            int totalFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories).Length;
            int processedFiles = 0;
            
            foreach (var file in sourceDir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                var exclude = false;
                
                updateCallback?.Invoke(file.Name, (int)Math.Floor(((double)processedFiles / totalFiles) * 100));
                
                foreach (var exclusion in exclusions)
                {
                    var currentFileRelativePath = file.FullName.Replace(sourceDir.FullName, "");
                    
                    if (currentFileRelativePath.StartsWith(exclusion) || currentFileRelativePath == exclusion)
                    {
                        exclude = true;
                        Log.Debug(
                            $"EXCLUSION FOUND :: FILE\nExclusion: '{exclusion}'\nPath: '{currentFileRelativePath}'");
                        break;
                    }
                    
                    if (currentFileRelativePath.EndsWith(".bak"))
                    {
                        exclude = true;
                        Log.Debug($"EXCLUDING BAK FILE :: {currentFileRelativePath}");
                        break;
                    }
                }
                
                if (exclude)
                    continue;
                
                
                var targetFile = file.FullName.Replace(sourceDir.FullName, targetDir.FullName);
                
                Log.Debug(
                    $"COPY\nSourceDir: '{sourceDir.FullName}'\nTargetDir: '{targetDir.FullName}'\nNewPath: '{targetFile}'");
                
                File.Copy(file.FullName, targetFile, true);
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
    
    public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir,
        IProgress<double> progress = null, string[] exclusions = null) =>
        CopyDirectoryWithProgress(sourceDir, targetDir, (msg, prog) => progress?.Report(prog), exclusions);
    
    public static Result CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir,
        Action<string, int> updateCallback = null, string[] exclusions = null)
    {
        try
        {
            var iterateDirectoriesResult = IterateDirectories(sourceDir, targetDir, exclusions ??= new string[0]);
            
            if (!iterateDirectoriesResult.Succeeded) return iterateDirectoriesResult;
            
            var iterateFilesResult = IterateFiles(sourceDir, targetDir, exclusions ??= new string[0], updateCallback);
            
            if (!iterateFilesResult.Succeeded) return iterateDirectoriesResult;
            
            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during directory copy");
            return Result.FromError(ex.Message);
        }
    }
    
    public static bool StreamAssemblyResourceOut(string resourceName, string outputFilePath)
    {
        try
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
            
            outputFile.Refresh();
            return outputFile.Exists;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"Failed to stream resource out: {resourceName}");
            return false;
        }
    }
    
    public static bool CheckPathForProblemLocations(string path, out PathCheck failedCheck)
    {
        failedCheck = new();
        
        var problemPaths = new List<PathCheck>()
        {
            new("Documents", PathCheckType.EndsWith, PathCheckAction.Warn),
            new("Desktop", PathCheckType.EndsWith, PathCheckAction.Deny),
            new("Battlestate Games", PathCheckType.Contains, PathCheckAction.Deny),
            new("Desktop", PathCheckType.Contains, PathCheckAction.Warn),
            new("scoped_dir", PathCheckType.Contains, PathCheckAction.Deny),
            new("Downloads", PathCheckType.Contains, PathCheckAction.Deny),
            new("OneDrive", PathCheckType.Contains, PathCheckAction.Deny),
            new("NextCloud", PathCheckType.Contains, PathCheckAction.Deny),
            new("DropBox", PathCheckType.Contains, PathCheckAction.Deny),
            new("Google", PathCheckType.Contains, PathCheckAction.Deny),
            new("Program Files", PathCheckType.Contains, PathCheckAction.Deny),
            new("Program Files (x86", PathCheckType.Contains, PathCheckAction.Deny),
            new(Path.Join("spt-installer", "cache"), PathCheckType.Contains, PathCheckAction.Deny),
            new("Drive Root", PathCheckType.DriveRoot, PathCheckAction.Deny)
        };
        
        foreach (var check in problemPaths)
        {
            switch (check.CheckType)
            {
                case PathCheckType.EndsWith:
                    if (path.ToLower().EndsWith(check.Target.ToLower()))
                    {
                        failedCheck = check;
                        return true;
                    }
                    
                    break;
                case PathCheckType.Contains:
                    if (path.ToLower().Contains(check.Target.ToLower()))
                    {
                        failedCheck = check;
                        return true;
                    }
                    
                    break;
                case PathCheckType.DriveRoot:
                    if (Regex.Match(path.ToLower(), @"^\w:(\\|\/)$").Success)
                    {
                        failedCheck = check;
                        return true;
                    }
                    
                    break;
            }
        }
        
        return false;
    }
}