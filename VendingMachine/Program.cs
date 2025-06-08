using System;
using VendingMachineApp.Implementations;
using VendingMachineApp.Interfaces;

namespace VendingMachineApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ICoinProcessor coinProcessor = new CoinProcessor();
            IProductDispenser productDispenser = new ProductDispenser();
            IChangeDispenser changeDispenser = new ChangeDispenser();
            IVendingMachineDisplay display = new ConsoleDisplay();
            IProductHandler state = new ProductHandler(); 
            VendingMachine machine = new VendingMachine(
                coinProcessor,
                productDispenser,
                changeDispenser,
                display,
                state);

            machine.Run();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}