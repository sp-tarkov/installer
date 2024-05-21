using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class FileHelper
{
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
            var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);
            var fileCopies = new List<CopyInfo>();
            int count = 0;
            
            // filter files before starting copy
            foreach (var file in allFiles)
            {
                count++;
                updateCallback?.Invoke("getting list of files to copy", (int)Math.Floor((double)count / allFiles.Length * 100));
                
                var currentFileRelativePath = file.FullName.Replace(sourceDir.FullName, "");

                if (exclusions != null)
                {
                    // check exclusions
                    foreach (var exclusion in exclusions)
                    {
                        if (currentFileRelativePath.StartsWith(exclusion) || currentFileRelativePath == exclusion)
                        {
                            Log.Debug(
                                $"EXCLUSION FOUND :: FILE\nExclusion: '{exclusion}'\nPath: '{currentFileRelativePath}'");
                            break;
                        }
                    }
                }

                // don't copy .bak files
                if (currentFileRelativePath.EndsWith(".bak"))
                {
                    Log.Debug($"EXCLUDING BAK FILE :: {currentFileRelativePath}");
                    break;
                }
                
                fileCopies.Add(new CopyInfo(file.FullName, file.FullName.Replace(sourceDir.FullName, targetDir.FullName)));
            }

            count = 0;
            
            // process copy info for files that need to be copied
            foreach (var copyInfo in fileCopies)
            {
                count++;
                updateCallback?.Invoke(copyInfo.FileName, (int)Math.Floor((double)count / fileCopies.Count * 100));
                
                var result = copyInfo.Copy();
                
                if (!result.Succeeded)
                {
                    return result;
                }
            }
            
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
            Log.Debug($"Starting StreamAssemblyResourceOut, resourcename: {resourceName}, outputFilePath: {outputFilePath}");
            var assembly = Assembly.GetExecutingAssembly();
            Log.Debug($"1");
            
            FileInfo outputFile = new FileInfo(outputFilePath);
            Log.Debug($"2");
            if (outputFile.Exists)
            {
                Log.Debug($"3");
                outputFile.Delete();
            }
            Log.Debug($"4");
            if (!outputFile.Directory.Exists)
            {
                Log.Debug($"5");
                Directory.CreateDirectory(outputFile.Directory.FullName);
            }
            Log.Debug($"6");
            var resName = assembly.GetManifestResourceNames().First(x => x.EndsWith(resourceName));
            Log.Debug($"7");
            using (FileStream fs = File.Create(outputFilePath))
            using (Stream s = assembly.GetManifestResourceStream(resName))
            {
                Log.Debug($"8");
                s.CopyTo(fs);
            }
            Log.Debug($"9");
            outputFile.Refresh();
            Log.Debug(outputFile.Exists.ToString());
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