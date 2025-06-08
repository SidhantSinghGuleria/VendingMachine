using System;
using System.Collections.Generic;
using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;

namespace VendingMachineApp.Implementations
{
    public class CoinProcessor : ICoinProcessor
    {
        private Dictionary<string, decimal> acceptedCoinValues = new Dictionary<string, decimal>
        {
            { "nickel", 0.05m },
            { "dime", 0.10m },
            { "quarter", 0.25m }
        };

        public Coin ProcessInsertedCoin(string coinType)
        {
            if (coinType.Equals("penny", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Rejected: Penny ($0.01). Returning coin.");
                return new Coin(coinType, 0.01m, false);
            }
            else if (acceptedCoinValues.TryGetValue(coinType, out decimal value))
            {
                Console.WriteLine($"Accepted: {coinType} (${value:F2}).");
                return new Coin(coinType, value, true);
            }
            else
            {
                Console.WriteLine($"Invalid coin type: '{coinType}'. Please use nickel, dime, or quarter.");
                return new Coin(coinType, 0.00m, false);
            }
        }
    }
}