using NUnit.Framework;
using Moq;
using VendingMachineApp;
using VendingMachineApp.Interfaces;
using VendingMachineApp.Models;
using System.Collections.Generic;
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

            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m);
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


        [Test]
        public void InsertCoin_ValidNickel_AddsToCurrentAmountAndUpdatesDisplay()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("nickel"))
                  .Returns(new Coin("nickel", 0.05m, true));
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m); _mockState.Setup(s => s.AddAmount(0.05m)).Callback(() => _mockState.SetupGet(s => s.CurrentAmount).Returns(0.05m));
            _vendingMachine.SimulateInput("insert nickel");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("nickel"), Times.Once);
            _mockState.Verify(s => s.AddAmount(0.05m), Times.Once);
            _mockDisplay.Verify(d => d.ShowCurrentAmount(0.05m), Times.Once);
            _mockDisplay.Verify(d => d.ShowMessage(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void InsertCoin_ValidDime_AddsToCurrentAmountAndUpdatesDisplay()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("dime"))
                  .Returns(new Coin("dime", 0.10m, true));
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m); _mockState.Setup(s => s.AddAmount(0.10m)).Callback(() => _mockState.SetupGet(s => s.CurrentAmount).Returns(0.10m));
            _vendingMachine.SimulateInput("insert dime");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("dime"), Times.Once);
            _mockState.Verify(s => s.AddAmount(0.10m), Times.Once);
            _mockDisplay.Verify(d => d.ShowCurrentAmount(0.10m), Times.Once);
        }

        [Test]
        public void InsertCoin_ValidQuarter_AddsToCurrentAmountAndUpdatesDisplay()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("quarter"))
                  .Returns(new Coin("quarter", 0.25m, true));
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m); _mockState.Setup(s => s.AddAmount(0.25m)).Callback(() => _mockState.SetupGet(s => s.CurrentAmount).Returns(0.25m));
            _vendingMachine.SimulateInput("insert quarter");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("quarter"), Times.Once);
            _mockState.Verify(s => s.AddAmount(0.25m), Times.Once);
            _mockDisplay.Verify(d => d.ShowCurrentAmount(0.25m), Times.Once);
        }

        [Test]
        public void InsertCoin_MultipleValidCoins_AccumulatesAmountAndUpdatesDisplay()
        {
            _mockCoinProcessor.SetupSequence(cp => cp.ProcessInsertedCoin(It.IsAny<string>()))
                  .Returns(new Coin("quarter", 0.25m, true))
                  .Returns(new Coin("dime", 0.10m, true));

            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m); _mockState.Setup(s => s.AddAmount(0.25m)).Callback(() => _mockState.SetupGet(s => s.CurrentAmount).Returns(0.25m));
            _mockState.Setup(s => s.AddAmount(0.10m)).Callback(() => _mockState.SetupGet(s => s.CurrentAmount).Returns(0.35m));

            _vendingMachine.SimulateInput("insert quarter");
            _vendingMachine.DisplayCurrentStatus(); _vendingMachine.SimulateInput("insert dime");
            _vendingMachine.DisplayCurrentStatus();
            _mockState.Verify(s => s.AddAmount(0.25m), Times.Once);
            _mockState.Verify(s => s.AddAmount(0.10m), Times.Once);
            _mockDisplay.Verify(d => d.ShowCurrentAmount(0.25m), Times.Once);
            _mockDisplay.Verify(d => d.ShowCurrentAmount(0.35m), Times.Once);
        }


        [Test]
        public void InsertCoin_InvalidPenny_DoesNotAddToCurrentAmountAndDoesNotChangeDisplayFromInsertCoin()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("penny"))
                  .Returns(new Coin("penny", 0.01m, false)); _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m);
            _vendingMachine.SimulateInput("insert penny");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("penny"), Times.Once);
            _mockState.Verify(s => s.AddAmount(It.IsAny<decimal>()), Times.Never); _mockDisplay.Verify(d => d.ShowMessage("INSERT COIN"), Times.AtLeastOnce); _mockDisplay.Verify(d => d.ShowCurrentAmount(It.IsAny<decimal>()), Times.Never);
        }

        [Test]
        public void InsertCoin_InvalidPenny_DoesNotAddToCurrentAmount_WhenAlreadyHasMoney()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("penny"))
                  .Returns(new Coin("penny", 0.01m, false));
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.50m); _mockState.Setup(s => s.GetAllProducts()).Returns(new Dictionary<string, Product>());
            _vendingMachine.SimulateInput("insert penny");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("penny"), Times.Once);
            _mockState.Verify(s => s.AddAmount(It.IsAny<decimal>()), Times.Never); _mockDisplay.Verify(d => d.ShowCurrentAmount(0.50m), Times.Once);
            _mockDisplay.Verify(d => d.ShowMessage(It.IsAny<string>()), Times.Never);
        }


        [Test]
        public void InsertCoin_UnknownCoinType_DoesNotAddToCurrentAmount()
        {
            _mockCoinProcessor.Setup(cp => cp.ProcessInsertedCoin("invalidcoin"))
                  .Returns(new Coin("invalidcoin", 0.00m, false)); _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m);

            _vendingMachine.SimulateInput("insert invalidcoin");
            _vendingMachine.DisplayCurrentStatus();

            _mockCoinProcessor.Verify(cp => cp.ProcessInsertedCoin("invalidcoin"), Times.Once);
            _mockState.Verify(s => s.AddAmount(It.IsAny<decimal>()), Times.Never);
            _mockDisplay.Verify(d => d.ShowMessage("INSERT COIN"), Times.AtLeastOnce);
        }


        [Test]
        public void SelectProduct_EnoughMoney_DispensesProductAndReturnsChangeAndResetsAmount()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(1.00m); _mockState.Setup(s => s.GetProductPrice("cola")).Returns(1.00m);

            _vendingMachine.SimulateInput("select cola");
            _vendingMachine.DisplayCurrentStatus();

            _mockProductDispenser.Verify(pd => pd.DispenseProduct("cola"), Times.Once);
            _mockChangeDispenser.Verify(cd => cd.DispenseChange(0.00m), Times.Once); _mockState.Verify(s => s.ResetAmount(), Times.Once);
            _mockDisplay.Verify(d => d.ShowMessage("THANK YOU"), Times.Once);
        }

        [Test]
        public void SelectProduct_EnoughMoney_DispensesProductAndReturnsCorrectChange()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(1.25m); _mockState.Setup(s => s.GetProductPrice("cola")).Returns(1.00m);
            _mockState.Setup(s => s.DeductAmount(1.00m));
            _vendingMachine.SimulateInput("select cola");
            _vendingMachine.DisplayCurrentStatus();

            _mockProductDispenser.Verify(pd => pd.DispenseProduct("cola"), Times.Once);
            _mockChangeDispenser.Verify(cd => cd.DispenseChange(0.25m), Times.Once); _mockState.Verify(s => s.ResetAmount(), Times.Once);
            _mockDisplay.Verify(d => d.ShowMessage("THANK YOU"), Times.Once);
        }


        [Test]
        public void SelectProduct_NotEnoughMoney_DisplaysPriceAndDoesNotDispense()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.25m); _mockState.Setup(s => s.GetProductPrice("cola")).Returns(1.00m);

            _vendingMachine.SimulateInput("select cola");
            _vendingMachine.DisplayCurrentStatus();

            _mockChangeDispenser.Verify(cd => cd.DispenseChange(It.IsAny<decimal>()), Times.Never); _mockState.Verify(s => s.ResetAmount(), Times.Never); _mockDisplay.Verify(d => d.ShowMessage("PRICE $1.00"), Times.Once);
        }

        [Test]
        public void SelectProduct_InvalidProductName_DisplaysInvalidProductAndDoesNotDispense()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(1.00m); _mockState.Setup(s => s.GetAllProducts()).Returns(new Dictionary<string, Product>());
            _vendingMachine.SimulateInput("select unknown_product");
            _vendingMachine.DisplayCurrentStatus();

            _mockProductDispenser.Verify(pd => pd.DispenseProduct(It.IsAny<string>()), Times.Never);
            _mockDisplay.Verify(d => d.ShowMessage("INVALID PRODUCT"), Times.Once); _mockState.Verify(s => s.ResetAmount(), Times.Never);
        }


        [Test]
        public void ReturnCoins_WithMoney_DispensesChangeAndResetsAmount()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.75m);
            _vendingMachine.SimulateInput("return");
            _vendingMachine.DisplayCurrentStatus();

            _mockChangeDispenser.Verify(cd => cd.DispenseChange(0.75m), Times.Once);
            _mockState.Verify(s => s.ResetAmount(), Times.Once);
        }


        [Test]
        public void ReturnCoins_NoMoney_DoesNothing()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(0.00m);
            _vendingMachine.SimulateInput("return");
            _vendingMachine.DisplayCurrentStatus();

            _mockChangeDispenser.Verify(cd => cd.DispenseChange(It.IsAny<decimal>()), Times.Never); _mockState.Verify(s => s.ResetAmount(), Times.Never); _mockDisplay.Verify(d => d.ShowMessage("INSERT COIN"), Times.AtLeastOnce);
        }


        [Test]
        public void Display_AfterSuccessfulTransaction_ShowsThankYouThenInsertCoin()
        {
            _mockState.SetupGet(s => s.CurrentAmount).Returns(1.00m);
            _mockState.Setup(s => s.GetProductPrice("cola")).Returns(1.00m);
            _mockState.Setup(s => s.DeductAmount(It.IsAny<decimal>()));
            _mockState.Setup(s => s.ResetAmount());
            _vendingMachine.SimulateInput("select cola");
            _vendingMachine.DisplayCurrentStatus();
            _mockDisplay.Verify(d => d.ShowMessage("THANK YOU"), Times.Once);
            _vendingMachine.DisplayCurrentStatus();

        }
    }
}