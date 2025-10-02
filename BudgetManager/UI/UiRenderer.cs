using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetManager.UI
{
    internal class UiRenderer
    {
        public void DrawUiFrame(string title, IEnumerable<string> lines, bool showIndex = false, string? footer = null)
        {
            Console.Clear();

            int contentWidth = 0;
            if (lines.Any())
            {
                int indexWidth = showIndex ? (lines.Count().ToString().Length + 2) : 0;
                contentWidth = lines.Max(line => line.Length + indexWidth);
            }

            int titleWidth = title.Length;
            int footerWidth = string.IsNullOrWhiteSpace(footer) ? 0 : footer.Length;

            int finalWidth = Math.Max(Math.Max(contentWidth, titleWidth), footerWidth);
            finalWidth = Math.Clamp(finalWidth, 30, 100);


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" {new string('=', finalWidth)} ");
            Console.ResetColor();

            int titlePadding = (finalWidth - title.Length) / 2;
            Console.WriteLine($"{new string(' ', Math.Max(0, titlePadding))}{title}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" {new string('=', finalWidth)} ");
            Console.ResetColor();

            int counter = 1;
            foreach (string line in lines)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("|");
                Console.ResetColor();

                string content = showIndex ? $"{counter}. {line}" : line;
                Console.Write(content.PadRight(finalWidth));

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("|");
                Console.ResetColor();

                counter++;
            }


            if (!string.IsNullOrWhiteSpace(footer))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"|{new string('=', finalWidth)}|");
                Console.ResetColor();

                int leftPadding = Math.Max(0, (finalWidth - footer.Length) / 2);
                int rightPadding = Math.Max(0, finalWidth - footer.Length - leftPadding);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("|");
                Console.ResetColor();

                Console.Write(new string(' ', leftPadding));
                Console.Write(footer);
                Console.Write(new string(' ', rightPadding));

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("|");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" {new string('=', finalWidth)} ");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" {new string('=', finalWidth)}");
                Console.ResetColor();
            }

        }

        private void ShowMessage(string message, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        public void ShowError(string message) => ShowMessage(message, ConsoleColor.Red);
        public void ShowSuccess(string message) => ShowMessage(message, ConsoleColor.Green);
        public void ShowInfo(string message) => ShowMessage(message, ConsoleColor.Yellow);

        public void ReturnToMenu()
        {
            Console.WriteLine("\nKliknij 'p', aby powrócić do menu.");
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.P)
                {
                    return;
                }
            }
        }
    }
}
