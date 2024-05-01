using ReactiveUI;
using Serilog;
using SPTInstaller.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SPTInstaller.Models;

public class InstallerUpdateInfo : ReactiveObject
{
    private Version? _newVersion;
    
    public string ChangeLog = "";
    
    private string _updateInfoText = "";
    
    public string UpdateInfoText
    {
        get => _updateInfoText;
        set => this.RaiseAndSetIfChanged(ref _updateInfoText, value);
    }
    
    private bool _show = false;
    
    public bool Show
    {
        get => _show;
        set => this.RaiseAndSetIfChanged(ref _show, value);
    }
    
    private bool _updating = false;
    
    public bool Updating
    {
        get => _updating;
        set => this.RaiseAndSetIfChanged(ref _updating, value);
    }
    
    private bool _updateAvailable = false;
    
    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => this.RaiseAndSetIfChanged(ref _updateAvailable, value);
    }
    
    private bool _checkingForUpdates = false;
    
    public bool CheckingForUpdates
    {
        get => _checkingForUpdates;
        set => this.RaiseAndSetIfChanged(ref _checkingForUpdates, value);
    }
    
    private int _downloadProgress;
    
    public int DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }
    
    public async Task UpdateInstaller()
    {
        Updating = true;
        UpdateAvailable = false;
        
        var updater = new FileInfo(Path.Join(DownloadCacheHelper.CachePath, "update.ps1"));
        
        if (!FileHelper.StreamAssemblyResourceOut("update.ps1", updater.FullName))
        {
            Log.Fatal("Failed to prepare update file");
            return;
        }
        
        
        if (!updater.Exists)
        {
            UpdateInfoText = "Failed to get updater from resources :(";
            return;
        }
        
        var newInstallerPath = await DownloadNewInstaller();
        
        if (string.IsNullOrWhiteSpace(newInstallerPath))
            return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            ArgumentList =
            {
                "-ExecutionPolicy", "Bypass", "-File", $"{updater.FullName}", $"{newInstallerPath}",
                $"{Path.Join(Environment.CurrentDirectory, "SPTInstaller.exe")}"
            }
        });
    }
    
    private async Task<string> DownloadNewInstaller()
    {
        UpdateInfoText = $"Downloading installer v{_newVersion}";
        
        var progress = new Progress<double>(x => DownloadProgress = (int)x);
        
        var file = await DownloadCacheHelper.DownloadFileAsync("SPTInstaller.exe", DownloadCacheHelper.InstallerUrl,
            progress);
        
        if (file == null || !file.Exists)
        {
            UpdateInfoText = "Failed to download new installer :(";
            return "";
        }
        
        return file.FullName;
    }
    
    private void EndCheck(string infoText, bool updateAvailable, bool log = true)
    {
        if (log)
        {
            Log.Information(infoText);
        }
        
        UpdateInfoText = infoText;
        Show = updateAvailable;
        CheckingForUpdates = false;
        UpdateAvailable = updateAvailable;
    }
    
    public async Task CheckForUpdates(Version? currentVersion)
    {
        if (currentVersion == null)
            return;
        
        UpdateInfoText = "Checking for installer updates";
        Show = true;
        CheckingForUpdates = true;
        
        try
        {
            var installerInfoFile =
                await DownloadCacheHelper.DownloadFileAsync("installer.json", DownloadCacheHelper.InstallerInfoUrl,
                    null);
            
            if (installerInfoFile == null)
            {
                EndCheck("Failed to download installer info", false);
                return;
            }
            
            var installerInfo =
                JsonConvert.DeserializeObject<InstallerInfo>(File.ReadAllText(installerInfoFile.FullName));
            
            if (installerInfo == null)
            {
                EndCheck("Failed to parse installer info json", false);
                return;
            }
            
            var latestVersion = new Version(installerInfo.LatestVersion);
            
            if (latestVersion <= currentVersion)
            {
                EndCheck("No updates available", false);
                return;
            }
            
            _newVersion = latestVersion;
            ChangeLog = installerInfo.ChangeLog;
            
            EndCheck($"Update available: v{latestVersion}", true);
            
            return;
        }
        catch (Exception ex)
        {
            EndCheck(ex.Message, false, false);
            Log.Error(ex, "Failed to check for updates");
        }
        
        return;
    }
}