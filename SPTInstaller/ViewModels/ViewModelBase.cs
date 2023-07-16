using Avalonia.Threading;
using ReactiveUI;
using System.Threading.Tasks;

namespace SPTInstaller.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel, IRoutableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    public string? UrlPathSegment => Guid.NewGuid().ToString().Substring(0, 7);

    public IScreen HostScreen { get; }

    /// <summary>
    /// Delay the return of the viewmodel
    /// </summary>
    /// <param name="Milliseconds">The amount of time in milliseconds to delay</param>
    /// <returns>The viewmodel after the delay time</returns>
    /// <remarks>Useful to delay the navigation to another view. For instance, to allow an animation to complete.</remarks>
    private async Task<ViewModelBase> WithDelay(int Milliseconds)
    {
        await Task.Delay(Milliseconds);

        return this;
    }

    /// <summary>
    /// Navigate to another viewmodel after a delay
    /// </summary>
    /// <param name="ViewModel"></param>
    /// <param name="Milliseconds"></param>
    /// <returns></returns>
    public async Task NavigateToWithDelay(ViewModelBase ViewModel, int Milliseconds)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            HostScreen.Router.Navigate.Execute(await ViewModel.WithDelay(Milliseconds));
        });
    }

    /// <summary>
    /// Navigate to another viewmodel
    /// </summary>
    /// <param name="ViewModel"></param>
    public void NavigateTo(ViewModelBase ViewModel)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            HostScreen.Router.Navigate.Execute(ViewModel);
        });
    }

    public ViewModelBase(IScreen Host)
    {
        HostScreen = Host;
    }
}