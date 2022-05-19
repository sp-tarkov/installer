using Spectre.Console;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class LogHelper
    {
        private static void Log(string tag, string message, string foreground, string background = "black")
        {
            AnsiConsole.MarkupLine($"[{foreground} on {background}][[{tag}]]: {message.EscapeMarkup()}[/]");
        }

        /// <summary>
        /// Outputs a string to console starting with [USER] with
        /// Green text
        /// </summary>
        public static void User(string text)
        {
            Log("USER", text, "green");
        }

        /// <summary>
        /// Outputs a string to console starting with [WARNING] with
        /// Yellow text
        /// </summary>
        public static void Warning(string text)
        {
            Log("WARNING", text, "yellow");
        }

        /// <summary>
        /// Outputs a string to console starting with [ERROR] with
        /// Red text
        /// </summary>
        public static void Error(string text)
        {
            Log("ERROR", text, "red");
        }

        /// <summary>
        /// Outputs a string to console starting with [INFO] with
        /// Blue text
        /// </summary>
        public static void Info(string text)
        {
            Log("INFO", text, "blue");
        }
    }
}
