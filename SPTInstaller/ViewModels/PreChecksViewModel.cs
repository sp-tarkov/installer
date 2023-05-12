using ReactiveUI;
using SPTInstaller.Controllers;
using SPTInstaller.Helpers;
using SPTInstaller.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SPTInstaller.ViewModels
{
    public class PreChecksViewModel : ViewModelBase
    {
        private string _InstallPath;
        public string InstallPath
        {
            get => _InstallPath;
            set => this.RaiseAndSetIfChanged(ref _InstallPath, value);
        }

        ObservableCollection<PreCheckBase> PreChecks { get; set; } 
            = new ObservableCollection<PreCheckBase>(ServiceHelper.GetAll<PreCheckBase>());

        ICommand StartInstallCommand { get; set; }

        public PreChecksViewModel(IScreen host) : base(host)
        {
            var data = ServiceHelper.Get<InternalData>();
            var installer = ServiceHelper.Get<InstallController>();

            if(data == null || installer == null)
            {
                // TODO: abort to message view
            }

            data.TargetInstallPath = Environment.CurrentDirectory;

            InstallPath = data.TargetInstallPath;

            StartInstallCommand = ReactiveCommand.Create(() =>
            {
                NavigateTo(new InstallViewModel(HostScreen));
            });


            Task.Run(async () =>
            {
                // TODO: if a required precheck fails, abort to message view
                var result = await installer.RunPreChecks();
            });
        }
    }
}
