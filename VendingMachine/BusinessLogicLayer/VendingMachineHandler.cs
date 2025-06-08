using System;
using System.Globalization;
using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;
namespace VendingMachineApp
{
    public class VendingMachine
    {
        private readonly ICoinProcessor _coinProcessor;
        private readonly IProductDispenser _productDispenser;
        private readonly IChangeDispenser _changeDispenser;
        private readonly IVendingMachineDisplay _display;
        private readonly IProductHandler _state;
        public VendingMachine(ICoinProcessor coinProcessor, IProductDispenser productDispenser, IChangeDispenser changeDispenser, IVendingMachineDisplay display, IProductHandler state)
        {
            _coinProcessor = coinProcessor ?? throw new ArgumentNullException(nameof(coinProcessor));
            _productDispenser = productDispenser ?? throw new ArgumentNullException(nameof(productDispenser));
            _changeDispenser = changeDispenser ?? throw new ArgumentNullException(nameof(changeDispenser));
            _display = display ?? throw new ArgumentNullException(nameof(display));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }
        public void Run()
        {
            Console.WriteLine("--- Vending Machine Simulator ---");
            Console.WriteLine("Products:");
            foreach (var productEntry in _state.GetAllProducts())
            {
                Console.WriteLine($"  - {productEntry.Value.Name} (${productEntry.Value.Price:F2})");
            }
            Console.WriteLine("Accepted Coins: nickel ($0.05), dime ($0.10), quarter ($0.25)");
            Console.WriteLine("Rejected Coin: penny ($0.01)");
            Console.WriteLine("---------------------------------");

            DisplayCurrentStatus();
            string input;
            while (true)
            {
                Console.WriteLine("\nEnter 'insert <coin>' (e.g., 'insert quarter') or 'select <product>' (e.g., 'select cola') or 'return' or 'exit'.");
                Console.Write("> ");
                input = Console.ReadLine()?.ToLower().Trim();

                if (input == "exit")
                {
                    break;
                }
                else if (input.StartsWith("insert "))
                {
                    string coinType = input.Substring("insert ".Length);
                    HandleCoinInsertion(coinType);
                }
                else if (input.StartsWith("select "))
                {
                    string productName = input.Substring("select ".Length);
                    HandleProductSelection(productName);
                }
                else if (input == "return")
                {
                    HandleCoinReturn();
                }
                else
                {
                    Console.WriteLine("Invalid command. Please try again.");
                }

                DisplayCurrentStatus();
            }

            Console.WriteLine("Thank you for using the Vending Machine Simulator!");
        }
        public void DisplayCurrentStatus()
        {
            if (_state.CurrentAmount == 0.00m)
            {
                _display.ShowMessage("INSERT COIN");
            }
            else
            {
                _display.ShowCurrentAmount(_state.CurrentAmount);
            }

        }

        private void HandleCoinInsertion(string coinType)
        {
            Coin processedCoin = _coinProcessor.ProcessInsertedCoin(coinType);

            if (processedCoin.IsValid)
            {
                _state.AddAmount(processedCoin.Value);
            }
        }
        private void HandleProductSelection(string productName)
        {
            if (!_state.GetAllProducts().ContainsKey(productName))
            {
                Console.WriteLine($"INVALID PRODUCT");
                Console.WriteLine($"Product '{productName}' does not exist.");
                return;
            }

            decimal price = _state.GetProductPrice(productName);

            if (_state.CurrentAmount >= price)
            {
                _productDispenser.DispenseProduct(productName);

                decimal change = _state.CurrentAmount - price;
                _state.DeductAmount(price);
                _changeDispenser.DispenseChange(change);
                _state.ResetAmount();
                Console.WriteLine("THANK YOU");
            }
            else
            {
                Console.WriteLine($"PRICE ${price:F2}");
                Console.WriteLine($"Not enough money for {productName}. Price: ${price:F2}. Current: ${_state.CurrentAmount:F2}.");
            }
        }
        private void HandleCoinReturn()
        {
            if (_state.CurrentAmount > 0)
            {
                _changeDispenser.DispenseChange(_state.CurrentAmount);
                _state.ResetAmount();
            }
            else
            {
                Console.WriteLine("No coins to return.");
            }
        }
        public void ProcessInput(string input)
        {
            if (input.StartsWith("insert "))
            {
                string coinType = input.Substring("insert ".Length);
                HandleCoinInsertion(coinType);
            }
            else if (input.StartsWith("select "))
            {
                string productName = input.Substring("select ".Length);
                HandleProductSelection(productName);
            }
            else if (input == "return")
            {
                HandleCoinReturn();
            }
        }
    }
}