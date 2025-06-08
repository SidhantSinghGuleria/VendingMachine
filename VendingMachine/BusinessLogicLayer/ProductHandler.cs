using System.Collections.Generic;
using System.Linq;
using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;
using VendingMachineApp.Data;

namespace VendingMachineApp.Implementations
{
    public class ProductHandler : IProductHandler
    {
        private Dictionary<string, Product> products;
        public decimal CurrentAmount { get; private set; } = 0.00m;
        public ProductHandler()
        {
            products = ProductData.ProductPriceList.ToDictionary();
        }
        public void AddAmount(decimal value)
        {
            CurrentAmount += value;
        }
        public void DeductAmount(decimal value)
        {
            CurrentAmount -= value;
        }
        public void ResetAmount()
        {
            CurrentAmount = 0.00m;
        }
        public decimal GetProductPrice(string productName)
        {
            return products.TryGetValue(productName, out Product product) ? product.Price : 0.00m;
        }
        public Dictionary<string, Product> GetAllProducts()
        {
            return products.ToDictionary(entry => entry.Key, entry => new Product(entry.Value.Name, entry.Value.Price));
        }
    }
}