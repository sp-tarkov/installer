using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class SetupClientTask : LiveTableTask
    {
        private InternalData _data;

        public SetupClientTask(InternalData data) : base("Setup Client", false)
        {
            _data = data;
        }

        public override async Task<GenericResult> RunAsync()
        {
            /* TODO:
             * patch if needed
             * extract release
             */

            throw new NotImplementedException();
        }
    }
}
