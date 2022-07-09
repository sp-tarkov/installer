using Spectre.Console;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Interfaces
{
    public interface ILiveTaskTableEntry
    {
        public string TaskName { get; set; }

        public int RowIndex { get; set; }

        public void SetContext(LiveDisplayContext context, Table table);

        public Task<GenericResult> RunAsync();
    }
}
