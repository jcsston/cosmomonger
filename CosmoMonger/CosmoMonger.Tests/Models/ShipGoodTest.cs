﻿namespace CosmoMonger.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.Security;
    using CosmoMonger.Models;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    [TestFixture]
    public class ShipGoodTest : BasePlayerTest
    {
        [Test]
        public void Sell()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Setup player
            Player testPlayer = this.CreateTestPlayer();
            Ship testShip = testPlayer.Ship;
            CosmoSystem startingSystem = testShip.CosmoSystem;
            GameManager manager = new GameManager(testPlayer.User.UserName);
            
            // Store the player starting cash
            int playerStartingCash = testPlayer.Ship.Credits;

            // Add some water to this ship for us to sell
            Good water = (from g in db.Goods
                          where g.Name == "Water"
                          select g).SingleOrDefault();
            Assert.That(water, Is.Not.Null, "We should have a Water good");
            ShipGood shipGood = new ShipGood();
            shipGood.Good = water;
            shipGood.Quantity = 10;
            shipGood.Ship = testShip;
            testShip.ShipGoods.Add(shipGood);

            // Verify that the good is for sell in the current system
            SystemGood systemWater = startingSystem.GetGood(water.GoodId);
            if (systemWater == null)
            {
                startingSystem.AddGood(water.GoodId, 0);
                systemWater = startingSystem.GetGood(water.GoodId);
            }
            int playerProfit = systemWater.Price * 10;
            int systemStartingCount = systemWater.Quantity;
            shipGood.Sell(testShip, 10, systemWater.Price);

            Assert.That(systemWater.Quantity, Is.EqualTo(systemStartingCount + 10), "System should now have 10 more water goods");
            Assert.That(testPlayer.Ship.Credits, Is.EqualTo(playerStartingCash + playerProfit), "Player should have more cash credits now after selling");
        }

        [Test]
        [Explicit("Deletes Water good from Sol system")]
        public void SellNotSold()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Setup player
            Player testPlayer = this.CreateTestPlayer();
            Ship testShip = testPlayer.Ship;
            CosmoSystem startingSystem = testShip.CosmoSystem;
            GameManager manager = new GameManager(testPlayer.User.UserName);

            // Store the player starting cash
            int playerStartingCash = testPlayer.Ship.Credits;

            // Add some water to this ship for us to sell
            Good water = (from g in db.Goods
                          where g.Name == "Water"
                          select g).SingleOrDefault();
            Assert.That(water, Is.Not.Null, "We should have a Water good");
            ShipGood shipGood = new ShipGood();
            shipGood.Good = water;
            shipGood.Quantity = 10;
            shipGood.Ship = testShip;
            testShip.ShipGoods.Add(shipGood);

            // Verify that the good is for sell in the current system
            SystemGood systemWater = startingSystem.GetGood(water.GoodId);
            if (systemWater != null)
            {
                // Remove the good from the system
                db.SystemGoods.DeleteOnSubmit(systemWater);
                db.SubmitChanges();
            }
            try
            {
                shipGood.Sell(testShip, 10, systemWater.Price);
            }
            catch (InvalidOperationException ex)
            {
                // Correct
                Assert.That(ex, Is.Not.Null, "InvalidOperationException should be valid");
                return;
            }
            Assert.Fail("Player should not been able to sell in the system when the good was not sold in the system");
        }

        [Test]
        public void SellNotEnoughGoods()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Setup player
            Player testPlayer = this.CreateTestPlayer();
            Ship testShip = testPlayer.Ship;
            CosmoSystem startingSystem = testShip.CosmoSystem;
            GameManager manager = new GameManager(testPlayer.User.UserName);

            // Add some water to this ship for us to sell
            Good water = (from g in db.Goods
                          where g.Name == "Water"
                          select g).SingleOrDefault();
            Assert.That(water, Is.Not.Null, "We should have a Water good");
            ShipGood shipGood = new ShipGood();
            shipGood.Good = water;
            shipGood.Quantity = 10;
            shipGood.Ship = testShip;
            testShip.ShipGoods.Add(shipGood);
            db.SubmitChanges();

            // Verify that the good is for sell in the current system
            SystemGood systemWater = startingSystem.GetGood(water.GoodId);
            if (systemWater == null)
            {
                startingSystem.AddGood(water.GoodId, 0);
                systemWater = startingSystem.GetGood(water.GoodId);
            }

            try
            {
                shipGood.Sell(testShip, 20, systemWater.Price);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.That(ex.ParamName, Is.EqualTo("quantity"), "Quantity to sell should be the invalid argument");
                return;
            }

            Assert.Fail("Player should not been able to sell more goods than aboard");
        }
    }
}
