using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Interfaces
{
    internal interface IProgressableTask
    {
        public int Progress { get; set; }
    }
}
