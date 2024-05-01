using System.Windows.Input;
using ReactiveUI;
using SPTInstaller.CustomControls;

namespace SPTInstaller.Models;

public class PreCheckDetailInfo : ReactiveObject
{
    public PreCheckDetailInfo()
    {
        _name = "";
        _details = "";
        _actionButtonText = "";
        _showActionButton = false;
    }
    
    private string _name;
    
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    
    private string _details;
    
    public string Details
    {
        get => _details;
        set => this.RaiseAndSetIfChanged(ref _details, value);
    }
    
    private string _actionButtonText;
    
    public string ActionButtonText
    {
        get => _actionButtonText;
        set => this.RaiseAndSetIfChanged(ref _actionButtonText, value);
    }
    
    private ICommand _actionButtonCommand;
    
    public ICommand ActionButtonCommand
    {
        get => _actionButtonCommand;
        set => this.RaiseAndSetIfChanged(ref _actionButtonCommand, value);
    }
    
    private bool _showActionButton;
    
    public bool ShowActionButton
    {
        get => _showActionButton;
        set => this.RaiseAndSetIfChanged(ref _showActionButton, value);
    }
    
    private string _barColor;
    
    public string BarColor
    {
        get => _barColor;
        set => this.RaiseAndSetIfChanged(ref _barColor, value);
    }
}