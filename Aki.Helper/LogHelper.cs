using System;

namespace Installer.Aki.Helper
{
    public static class LogHelper
    {
        /// <summary>
        /// Outputs a string to console starting with [USER] with
        /// a Green background and Black foreground
        /// </summary>
        public static void User(string text)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"[USER]: {text}");
            Console.ResetColor();
        }

        /// <summary>
        /// Outputs a string to console starting with [WARNING] with
        /// a Yellow background and Black foreground
        /// </summary>
        public static void Warning(string text)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"[WARNING]: {text}");
            Console.ResetColor();
        }

        /// <summary>
        /// Outputs a string to console starting with [ERROR] with
        /// a Red background and Black foreground
        /// </summary>
        public static void Error(string text)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"[ERROR]: {text}");
            Console.ResetColor();
        }

        /// <summary>
        /// Outputs a string to console starting with [INFO] with
        /// a DarkGray background and White foreground
        /// </summary>
        public static void Info(string text)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[INFO]: {text}");
            Console.ResetColor();
        }
    }
}
