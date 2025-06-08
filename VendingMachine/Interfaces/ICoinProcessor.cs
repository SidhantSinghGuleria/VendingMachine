using System.Reflection;

namespace VendingMachineApp.Interfaces
{
    public interface ICoinProcessor
    {
        Models.Coin ProcessInsertedCoin(string coinType);
    }
}