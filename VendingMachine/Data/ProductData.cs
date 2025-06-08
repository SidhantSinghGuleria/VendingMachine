using System.Collections.Generic;
using VendingMachineApp.Models;

namespace VendingMachineApp.Data
{
    public static class ProductData
    {
        public static readonly Dictionary<string, Product> ProductPriceList = new Dictionary<string, Product>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "cola", new Product("cola", 1.00m) },
            { "chips", new Product("chips", 0.50m) },
            { "candy", new Product("candy", 0.65m) }
        };
    }
}