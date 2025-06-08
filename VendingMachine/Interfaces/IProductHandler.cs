using System.Reflection;

namespace VendingMachineApp.Interfaces
{
	public interface IProductHandler
	{
		decimal CurrentAmount { get; }
		void AddAmount(decimal value);
		void DeductAmount(decimal value);
		void ResetAmount();
		decimal GetProductPrice(string productName);
		Dictionary<string, Models.Product> GetAllProducts();
	}
}