using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.FileProviders;

namespace SPTInstaller;

public class EmbeddedBlazorWebView : BlazorWebView
{
    public bool UseEmbeddedResources { get; init; }
    public IFileProvider EmbeddedFilesProvider { get; set; }

    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        if (!UseEmbeddedResources)
        {
            return base.CreateFileProvider(contentRootDir);
        }

        EmbeddedFilesProvider = new ManifestEmbeddedFileProvider(typeof(App).Assembly, "Resources");
        return EmbeddedFilesProvider;
    }
}