using System.Linq;
using System.Security.Cryptography;
using Serilog;

namespace SPTInstaller.Helpers;

public static class FileHashHelper
{
    // public static string? GetGiteaReleaseHash(Release release)
    // {
    //     var regex = Regex.Match(release.Body, @"Release Hash: (?<hash>\S+)");
    //
    //     if (regex.Success)
    //     {
    //         return regex.Groups["hash"].Value;
    //     }
    //
    //     return null;
    // }

    public static bool CheckHash(FileInfo file, string expectedHash)
    {
        using var md5Service = MD5.Create();
        using var sourceStream = file.OpenRead();
        
        var sourceHash = md5Service.ComputeHash(sourceStream);
        var expectedHashBytes = Convert.FromBase64String(expectedHash);
        
        Log.Information($"Comparing Hashes :: S: {Convert.ToBase64String(sourceHash)} - E: {expectedHash}");

        var matched = Enumerable.SequenceEqual(sourceHash, expectedHashBytes);

        return matched;
    }
}