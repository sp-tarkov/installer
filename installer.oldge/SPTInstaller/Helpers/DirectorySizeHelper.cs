using System.Linq;
using Serilog;

namespace SPTInstaller.Helpers;

public static class DirectorySizeHelper
{
    // SizeSuffix implementation found here:
    // https://stackoverflow.com/a/14488941
    static readonly string[] SizeSuffixes =
        { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    
    public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException("decimalPlaces");
        }
        
        if (value < 0)
        {
            return "-" + SizeSuffix(-value, decimalPlaces);
        }
        
        if (value == 0)
        {
            return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
        }
        
        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);
        
        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));
        
        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }
        
        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }
    
    /// <summary>
    /// Gets the size of a directory in bytes
    /// </summary>
    /// <param name="sourceDir">The directory to get the size of</param>
    /// <returns>the size of the <paramref name="sourceDir"/> in bytes or -1 if an error occurred</returns>
    public static long GetSizeOfDirectory(DirectoryInfo sourceDir)
    {
        try
        {
            return sourceDir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Something went wrong calculating dir size");
            return -1;
        }
    }
}