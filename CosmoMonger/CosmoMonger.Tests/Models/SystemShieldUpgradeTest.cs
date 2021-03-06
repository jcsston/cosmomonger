﻿namespace CosmoMonger.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.Security;
    using CosmoMonger.Models;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    [TestFixture]
    public class SystemShieldUpgradeTest : BasePlayerTest
    {
        private SystemShieldUpgrade CreateSystemShieldUpgrade()
        {
            SystemShieldUpgrade upgrade = new SystemShieldUpgrade();
            upgrade.CosmoSystem = new CosmoSystem();
            upgrade.Shield = new Shield();
            upgrade.Shield.ShieldId = 1;
            upgrade.Shield.BasePrice = 1000;
            upgrade.Shield.CargoCost = 5;
            upgrade.PriceMultiplier = 0.75;
            upgrade.Quantity = 1;

            return upgrade;
        }

        [Test]
        public void PricePerLevel()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            // Act
            int price = upgrade.PricePerLevel;

            // Assert
            Assert.That(price, Is.EqualTo(750), "PricePerLevel should be 75% of 1000 credits");
        }

        [Test]
        public void GetPrice()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();
            Player testPlayer = this.CreateTestPlayer();

            // Act
            int price = upgrade.GetPrice(testPlayer.Ship);

            // Assert
            Assert.That(testPlayer.Ship.BaseShip.Level, Is.EqualTo(1), "Test player should have base level 1 ship");
            Assert.That(price, Is.EqualTo(750), "Price should be 750 credits for a level 1 ship");
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "credits")]
        public void BuyNotEnoughCredits()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();
            Mock<User> userMock = new Mock<User>();
            Mock<Ship> shipMock = new Mock<Ship>();

            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(1).Verifiable();

            Shield currentUpgrade = new Shield();
            // Trade value will be 80
            currentUpgrade.BasePrice = 100;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            // Cash on hand is 500
            shipMock.Expect(s => s.Credits)
                .Returns(500).AtMostOnce().Verifiable();

            // Act, should throw an exception
            upgrade.Buy(shipMock.Object);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "cargo space")]
        public void BuyNotEnoughCargoSpace()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();
            Mock<User> userMock = new Mock<User>();
            Mock<Ship> shipMock = new Mock<Ship>();

            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(1).Verifiable();

            Shield currentUpgrade = new Shield();
            // Trade value will be 80
            currentUpgrade.BasePrice = 100;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();
            
            // Cargo space free is 1
            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(1).AtMostOnce().Verifiable();
            
            // Cash on hand is 5000
            shipMock.Expect(s => s.Credits)
                .Returns(5000).AtMostOnce().Verifiable();

            // Act, should throw an exception
            upgrade.Buy(shipMock.Object);
        }

        [Test]
        public void Buy()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            Mock<Ship> shipMock = new Mock<Ship>();

            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(1).Verifiable();

            Shield currentUpgrade = new Shield();
            currentUpgrade.ShieldId = 2;
            // Trade value will be 80
            currentUpgrade.BasePrice = 100;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            // Cargo space free is 10
            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(10).Verifiable();

            // Cash on hand is 5000
            shipMock.Expect(s => s.Credits)
                .Returns(5000).Verifiable();

            // Act
            upgrade.Buy(shipMock.Object);

            // Assert
            shipMock.Verify();
            // Cost of the upgrade should be (750 - 80) credits
            shipMock.VerifySet(s => s.Credits, 5000 - (750 - 80));
            Assert.That(upgrade.Quantity, Is.EqualTo(0), "Should be no upgrades left in the system of this model");
        }

        [Test]
        public void BuyWithLevel2Ship()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            Mock<Ship> shipMock = new Mock<Ship>();

            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(2).Verifiable();

            Shield currentUpgrade = new Shield();
            currentUpgrade.ShieldId = 2;
            // Trade value will be 80
            currentUpgrade.BasePrice = 100;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            // Cargo space free is 10
            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(10).Verifiable();

            // Cash on hand is 5000
            shipMock.Expect(s => s.Credits)
                .Returns(5000).Verifiable();

            // Act
            upgrade.Buy(shipMock.Object);

            // Assert
            shipMock.Verify();
            // Cost of the upgrade should be (750*2 - 80*2) credits
            shipMock.VerifySet(s => s.Credits, 5000 - (750 * 2 - (80 * 2)));
            Assert.That(upgrade.Quantity, Is.EqualTo(0), "Should be no upgrades left in the system of this model");
        }

        [Test]
        public void BuyProfit()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            Mock<Ship> shipMock = new Mock<Ship>();

            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(1).Verifiable();

            Shield currentUpgrade = new Shield();
            // Trade value will be 3200
            currentUpgrade.BasePrice = 4000;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            // Cargo space free is 10
            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(10).AtMostOnce().Verifiable();

            // Cash on hand is 500
            shipMock.Expect(s => s.Credits)
                .Returns(500).AtMostOnce().Verifiable();

            // Act
            upgrade.Buy(shipMock.Object);

            // Assert
            shipMock.Verify();
            // Cost of the upgrade should be -1250 credits
            shipMock.VerifySet(s => s.Credits, 500 - (750-3200));
            Assert.That(upgrade.Quantity, Is.EqualTo(0), "Should be no upgrades left in the system of this model");
        }

        [Test]
        public void BuyPlayerUpgradeAddedToSystem()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            Mock<Ship> shipMock = new Mock<Ship>();
            shipMock.Expect(s => s.BaseShip.Level)
                .Returns(1).Verifiable();

            Shield currentUpgrade = new Shield();
            // Trade value will be 80
            currentUpgrade.BasePrice = 100;
            currentUpgrade.CargoCost = 1;

            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            // Cargo space free is 10
            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(10).AtMostOnce().Verifiable();

            // Cash on hand is 5000
            shipMock.Expect(s => s.Credits)
                .Returns(5000).Verifiable();


            // Act
            upgrade.Buy(shipMock.Object);

            // Assert
            shipMock.Verify();
            // Cost of the upgrade should be (750 - 80) credits
            shipMock.VerifySet(m => m.Credits, 5000 - (750 - 80));
            Assert.That(upgrade.Quantity, Is.EqualTo(0), "Should be no upgrades left in the system of this model");
            Assert.That(upgrade.CosmoSystem.SystemShieldUpgrades.Where(m => m.Shield == currentUpgrade && m.Quantity == 1).Any(), Is.True, "The players upgrade should have been added to the system for sale");
        }

        [Test]
        public void BuyPlayerUpgradeAddedToSystem2()
        {
            // Arrange
            SystemShieldUpgrade upgrade = this.CreateSystemShieldUpgrade();

            BaseShip playerBaseShip = new BaseShip();
            playerBaseShip.Level = 1;

            Shield currentUpgrade = new Shield();
            // Trade value will be 160
            currentUpgrade.BasePrice = 200;
            currentUpgrade.CargoCost = 1;

            SystemShieldUpgrade playerSystemUpgrade = new SystemShieldUpgrade();
            playerSystemUpgrade.Shield = currentUpgrade;
            playerSystemUpgrade.CosmoSystem = upgrade.CosmoSystem;
            playerSystemUpgrade.Quantity = 2;

            Mock<Ship> shipMock = new Mock<Ship>();

            // Setup player base upgrade model
            shipMock.Expect(s => s.BaseShip)
                .Returns(playerBaseShip).Verifiable();
            
            // Setup player base upgrade
            shipMock.Expect(s => s.Shield)
                .Returns(currentUpgrade).Verifiable();

            shipMock.Expect(s => s.CargoSpaceFree)
                .Returns(25).Verifiable();

            // Cash on hand is 5000
            shipMock.Expect(s => s.Credits)
                .Returns(5000).Verifiable();

            // Act
            upgrade.Buy(shipMock.Object);

            // Assert
            shipMock.Verify();
            // Cost of the upgrade should be (750 - 160) credits
            shipMock.VerifySet(m => m.Credits, 5000 - (750 - 160));
            Assert.That(upgrade.Quantity, Is.EqualTo(0), "Should be no ships left in the system of this model");
            Assert.That(upgrade.CosmoSystem.SystemShieldUpgrades.Where(m => m.Shield == playerSystemUpgrade.Shield && m.Quantity == 3).Any(), Is.True, "The players base ship should have been added to the system for sale");
        }
    }
}
