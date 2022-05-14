using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;

namespace SPT_AKI_Installer.Aki.Helper
{
    public class SpectreHelper
    {
        public void Figlet(string text)
        {
            AnsiConsole.Write(
                new FigletText(text)
                .Centered()
                .Color(Color.Yellow));
        }
    }
}
