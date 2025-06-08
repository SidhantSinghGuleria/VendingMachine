namespace VendingMachineApp.Interfaces
{
    public interface IVendingMachineDisplay
    {
        void ShowMessage(string message);
        void ShowCurrentAmount(decimal amount);    
    }
}