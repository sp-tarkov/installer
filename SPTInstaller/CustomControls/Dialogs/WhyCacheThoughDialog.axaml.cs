﻿using SPTInstaller.Helpers;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Serilog;
using SPTInstaller.Models;
using Color = System.Drawing.Color;

namespace SPTInstaller.CustomControls.Dialogs;

public partial class WhyCacheThoughDialog : UserControl
{
    private int _movePatcherState = 0;
    
    private FileInfo? _foundPatcher;
    
    public WhyCacheThoughDialog()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> AdditionalInfoProperty =
        AvaloniaProperty.Register<WhyCacheThoughDialog, string>(nameof(AdditionalInfo));
    
    public string AdditionalInfo
    {
        get => GetValue(AdditionalInfoProperty);
        set => SetValue(AdditionalInfoProperty, value);
    }
    
    public static readonly StyledProperty<string> AdditionalInfoColorProperty =
        AvaloniaProperty.Register<WhyCacheThoughDialog, string>(nameof(AdditionalInfoColor));
    
    public string AdditionalInfoColor
    {
        get => GetValue(AdditionalInfoColorProperty);
        set => SetValue(AdditionalInfoColorProperty, value);
    }
    
    
    public bool CacheExists => Directory.Exists(DownloadCacheHelper.CachePath);
    
    public void OpenCacheFolder()
    {
        if (!CacheExists)
            return;
        
        Process.Start(new ProcessStartInfo()
        {
            FileName = Path.EndsInDirectorySeparator(DownloadCacheHelper.CachePath)
                ? DownloadCacheHelper.CachePath
                : DownloadCacheHelper.CachePath + Path.DirectorySeparatorChar,
            UseShellExecute = true,
            Verb = "open"
        });
    }
    
    public void ClearCachedMetaData()
    {
        var cachedMetadata =
            new DirectoryInfo(DownloadCacheHelper.CachePath).GetFiles("*.json", SearchOption.TopDirectoryOnly);
        
        var message = "no cached metadata to remove";
        
        if (cachedMetadata.Length == 0)
        {
            AdditionalInfo = message;
            AdditionalInfoColor = "dodgerblue";
            Log.Information(message);
            return;
        }
        
        var allDeleted = true;
        
        foreach (var file in cachedMetadata)
        {
            try
            {
                file.Delete();
                file.Refresh();
                if (file.Exists)
                {
                    allDeleted = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to delete cached metadata file: {file.Name}");
            }
        }
        
        message = allDeleted ? "cached metadata removed" : "some files could not be removed. Check logs";
        AdditionalInfo = message;
        AdditionalInfoColor = allDeleted ? "green" : "red";
        Log.Information(message);
        
        var data = ServiceHelper.Get<InternalData>();
        
        App.ReLaunch(false, data.TargetInstallPath!);
    }
    
    public void MoveDownloadsPatcherToCache()
    {
        switch (_movePatcherState)
        {
            case 0:
                var downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
                
                var downloadsFolder = new DirectoryInfo(downloadsPath);
                
                if (!downloadsFolder.Exists)
                {
                    var message = "Could not get downloads folder :(";
                    Log.Error($"[MV_0] {message}");
                    AdditionalInfo = message;
                    AdditionalInfoColor = "red";
                    _movePatcherState = -1;
                    return;
                }
                
                _foundPatcher = downloadsFolder.GetFiles("Patcher_*").OrderByDescending(p => p.CreationTime)
                    .FirstOrDefault();
                
                if (_foundPatcher == null || !_foundPatcher.Exists)
                {
                    var message = "Could not find a patcher file in your downloads folder";
                    Log.Warning($"[MV_0] {message}");
                    AdditionalInfo = message;
                    
                    AdditionalInfoColor = "red";
                    return;
                }
                
                Log.Information($"[MV_0] Found patcher for move: {_foundPatcher.Name}");
                AdditionalInfo =
                    $"Click again to move the below patcher file to the cache folder\n{_foundPatcher?.Name ?? "-SOMETHING WENT WRONG-"}";
                AdditionalInfoColor = "#FFC107";
                _movePatcherState = 1;
                break;
            case 1:
                try
                {
                    var cacheFilePath = Path.Join(DownloadCacheHelper.CachePath, "patcher");
                    _foundPatcher?.MoveTo(cacheFilePath, true);
                    var message = "Patcher was moved into cache :D";
                    Log.Information($"[MV_1] {message}");
                    AdditionalInfo = message;
                    AdditionalInfoColor = "ForestGreen";
                }
                catch (Exception ex)
                {
                    AdditionalInfo = "Something went wrong :(";
                    AdditionalInfoColor = "red";
                    Log.Error(ex, "Failed to move downloaded patcher file into cache");
                }
                
                break;
            default:
                Log.Error("[MV_ ] Move state is broken :(");
                break;
        }
    }
}