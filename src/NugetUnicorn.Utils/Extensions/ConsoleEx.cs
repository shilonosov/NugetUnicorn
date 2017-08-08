using System;

namespace NugetUnicorn.Utils.Extensions
{
    public static class ConsoleEx
    {
        public static void WriteLine(ConsoleColor color, string format, params object[] parameters)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, parameters);
            //Debug.WriteLine(format, parameters);
            Console.ForegroundColor = currentColor;
        }
    }
}