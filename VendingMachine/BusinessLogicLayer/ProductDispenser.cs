using System;
using VendingMachineApp.Interfaces;

namespace VendingMachineApp.Implementations
{
    public class ProductDispenser : IProductDispenser
    {
        public void DispenseProduct(string productName)
        {
            Console.WriteLine($"Dispensing {productName}. Enjoy!");
        }
    }
}