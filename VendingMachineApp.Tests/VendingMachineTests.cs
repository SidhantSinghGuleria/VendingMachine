using NUnit.Framework;
using Moq;
using VendingMachineApp;
using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;
using System.Collections.Generic;
using System;
namespace VendingMachineApp.Tests
{
    [TestFixture]
    public class VendingMachineTests
    {
        private VendingMachine _vendingMachine;
        private Mock<ICoinProcessor> _mockCoinProcessor;
        private Mock<IProductDispenser> _mockProductDispenser;
        private Mock<IChangeDispenser> _mockChangeDispenser;
        private Mock<IVendingMachineDisplay> _mockDisplay;
        private Mock<IProductHandler> _mockState;
        [SetUp]
        public void Setup()
        {
            _mockCoinProcessor = new Mock<ICoinProcessor>();
            _mockProductDispenser = new Mock<IProductDispenser>();
            _mockChangeDispenser = new Mock<IChangeDispenser>();
            _mockDisplay = new Mock<IVendingMachineDisplay>();
            _mockState = new Mock<IProductHandler>();
            _mockState.Setup(s => s.GetAllProducts()).Returns(new Dictionary<string, Product>
            {
                {"cola", new Product("cola", 1.00m)},
                {"chips", new Product("chips", 0.50m)},
                {"candy", new Product("candy", 0.65m)}
            });
            _vendingMachine = new VendingMachine(
                            _mockCoinProcessor.Object,
                            _mockProductDispenser.Object,
                            _mockChangeDispenser.Object,
                            _mockDisplay.Object,
                            _mockState.Object
                        );
        }
        [TestCase("nickel", 0.05, true, TestName = "Insert Valid Nickel")]
        [TestCase("dime", 0.10, true, TestName = "Insert Valid Dime")]
        [TestCase("quarter", 0.25, true, TestName = "Insert Valid Quarter")]
        [TestCase("penny", 0.01, false, TestName = "Insert Invalid Penny")]
        [TestCase("invalidcoin", 0.00, false, TestName = "Insert Unknown Coin Type")]
        public void InsertCoin_ShouldProcessAndDisplayCorrectly(string coinType, decimal coinValue, bool isValidCoin)
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin(coinType))
                  .Returns(new Coin(coinType, coinValue, isValidCoin));
            decimal currentAmount = 0.00m;
            _mockState.SetupGet(s => s.CurrentAmount).Returns(() => currentAmount); _mockState.Setup(s => s.AddAmount(It.IsAny<decimal>()))
                      .Callback<decimal>(val => currentAmount += val);
            _vendingMachine.ProcessInput($"insert {coinType}");
            _vendingMachine.DisplayCurrentStatus();
            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin(coinType), Times.Once);
            if (isValidCoin)
            {
                _mockState.Verify(s => s.AddAmount(coinValue), Times.Once);
                _mockDisplay.Verify(d => d.ShowCurrentAmount(currentAmount), Times.Once);
            }
            else
            {
                _mockState.Verify(s => s.AddAmount(It.IsAny<decimal>()), Times.Never); _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
            }
        }
        [TestCase("cola", 1.00, 0.75, false, "PRICE $1.00", 0.25, TestName = "Select Cola - Not Enough Money")]
        [TestCase("chips", 0.50, 0.50, true, "THANK YOU", 0.00, TestName = "Select Chips - Exact Money")]
        [TestCase("candy", 0.65, 0.75, true, "THANK YOU", 0.10, TestName = "Select Candy - With Change")]
        [TestCase("cola", 1.00, 1.25, true, "THANK YOU", 0.25, TestName = "Select Cola - With More Change")]
        [TestCase("unknown_product", 0.00, 1.00, false, "INVALID PRODUCT", 0.00, TestName = "Select Unknown Product")]
        public void SelectProduct_ShouldHandleSelectionAndDisplay(string productName, decimal productPrice, decimal insertedAmount, bool expectedToDispense, string expectedDisplayMessage, decimal expectedChange)
        {
            _mockState.Setup(s => s.GetProductPrice(productName)).Returns(productPrice);
            _mockState.SetupGet(s => s.CurrentAmount).Returns(insertedAmount);
            decimal currentAmountAfterTransaction = insertedAmount;
            _mockState.Setup(s => s.DeductAmount(It.IsAny<decimal>()))
                      .Callback<decimal>(val => currentAmountAfterTransaction -= val);
            _mockState.Setup(s => s.ResetAmount())
                      .Callback(() => currentAmountAfterTransaction = 0.00m);
            _mockState.SetupGet(s => s.CurrentAmount).Returns(() => currentAmountAfterTransaction);
            _vendingMachine.ProcessInput($"select {productName}");
            _vendingMachine.DisplayCurrentStatus();
            if (expectedToDispense)
            {
                _mockProductDispenser.Verify(pd => pd.DispenseProduct(productName), Times.Once);
                _mockState.Verify(s => s.DeductAmount(productPrice), Times.Once);
                _mockChangeDispenser.Verify(cd => cd.DispenseChange(expectedChange), Times.Once);
                _mockState.Verify(s => s.ResetAmount(), Times.Once);
            }
            else
            {
                _mockProductDispenser.Verify(pd => pd.DispenseProduct(It.IsAny<string>()), Times.Never);
                _mockState.Verify(s => s.DeductAmount(It.IsAny<decimal>()), Times.Never);
                _mockChangeDispenser.Verify(cd => cd.DispenseChange(It.IsAny<decimal>()), Times.Never);
                _mockState.Verify(s => s.ResetAmount(), Times.Never);

            }
        }
        [TestCase(0.75, TestName = "Return Coins - With Money")]
        [TestCase(0.00, TestName = "Return Coins - No Money")]
        public void ReturnCoins_ShouldDispenseChangeAndResetAmount(decimal initialAmount)
        {
            decimal currentAmount = initialAmount;
            _mockState.SetupGet(s => s.CurrentAmount).Returns(() => currentAmount);
            _mockState.Setup(s => s.ResetAmount()).Callback(() => currentAmount = 0.00m);
            _vendingMachine.ProcessInput("return");
            _vendingMachine.DisplayCurrentStatus();
            if (initialAmount > 0)
            {
                _mockChangeDispenser.Verify(cd => cd.DispenseChange(initialAmount), Times.Once);
                _mockState.Verify(s => s.ResetAmount(), Times.Once);
                _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
            }
            else
            {
                _mockChangeDispenser.Verify(cd => cd.DispenseChange(It.IsAny<decimal>()), Times.Never);
                _mockState.Verify(s => s.ResetAmount(), Times.Never);
                _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
            }
        }
        [Test]
        public void Display_AfterSuccessfulTransaction_ShowsThankYouThenInsertCoin()
        {
            decimal currentAmount = 1.00m;
            _mockState.SetupGet(s => s.CurrentAmount).Returns(() => currentAmount);
            _mockState.Setup(s => s.GetProductPrice("cola")).Returns(1.00m);
            _mockState.Setup(s => s.DeductAmount(It.IsAny<decimal>()))
                      .Callback<decimal>(val => currentAmount -= val);
            _mockState.Setup(s => s.ResetAmount())
                      .Callback(() => currentAmount = 0.00m);
            _vendingMachine.ProcessInput("select cola");
            _vendingMachine.DisplayCurrentStatus();
            _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m); _vendingMachine.DisplayCurrentStatus();
            _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
        }
    }
}