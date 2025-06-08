namespace VendingMachineApp.Models
{
    public class Coin
    {
        public string Name { get; }
        public decimal Value { get; }
        public bool IsValid { get; }

        public Coin(string name, decimal value, bool isValid)
        {
            Name = name;
            Value = value;
            IsValid = isValid;
        }
    }
}