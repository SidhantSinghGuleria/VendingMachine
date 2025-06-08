using System;
using System.Globalization;
using VendingMachineApp.Interfaces;

namespace VendingMachineApp.Implementations
{
    public class ConsoleDisplay : IVendingMachineDisplay
    {
        public void ShowMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"DISPLAY: {message}");
            Console.ResetColor();
        }

        public void ShowCurrentAmount(decimal amount)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"DISPLAY: {amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}");
            Console.ResetColor();
        }
    }
}