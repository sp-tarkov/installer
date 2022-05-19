using Spectre.Console;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class SpectreHelper
    {
        public static void Figlet(string text, Color color)
        {
            AnsiConsole.Write(
                new FigletText(text)
                .Centered()
                .Color(color));
        }
    }
}
