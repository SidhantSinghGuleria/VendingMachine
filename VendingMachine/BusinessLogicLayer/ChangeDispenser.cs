using System;
using System.Globalization;
using VendingMachineApp.Interfaces;

namespace VendingMachineApp.Implementations
{
    public class ChangeDispenser : IChangeDispenser
    {
        public void DispenseChange(decimal amount)
        {
            if (amount > 0)
            {
                Console.WriteLine($"Returning change: {amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"))}.");
            }
        }
    }
}