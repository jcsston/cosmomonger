﻿namespace CosmoMonger.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.Security;
    using CosmoMonger.Models;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    /// <summary>
    /// Summary description for ShipTest
    /// </summary>
    [TestFixture]
    public class ShipTest : BasePlayerTest
    {
        [Test]
        public void GetInRangeSystems()
        {
            Player testPlayer = this.CreateTestPlayer();

            CosmoSystem [] inRangeSystems =  testPlayer.Ship.GetInRangeSystems();

            Assert.That(inRangeSystems, Is.Not.CollectionContaining(testPlayer.Ship.CosmoSystem), "In range systems should not include current system");
        }

        [Test]
        public void CargoSpaceFree()
        {
            // Arrange
            Ship ship = new Ship();
            ship.BaseShip = new BaseShip();
            ship.BaseShip.CargoSpace = 30;

            ship.JumpDrive = new JumpDrive();
            ship.JumpDrive.CargoCost = 1;

            ship.Shield = new Shield();
            ship.Shield.CargoCost = 2;

            ship.Weapon = new Weapon();
            ship.Weapon.CargoCost = 3;

            // Assert
            Assert.That(ship.CargoSpaceFree, Is.EqualTo(30-6), "Total free cargo space should equal the BaseShip amount minus 6");
        }

        [Test]
        public void CargoSpaceTotal()
        {
            // Arrange
            BaseShip baseShip = new BaseShip();
            baseShip.CargoSpace = 30;
            
            Ship ship = new Ship();
            ship.BaseShip = baseShip;

            // Assert
            Assert.That(ship.CargoSpaceTotal, Is.EqualTo(30), "Total cargo space should equal the BaseShip amount");
        }

        [Test]
        public void TradeInValue()
        {
            // Arrange
            Ship ship = new Ship();
            ship.BaseShip = new BaseShip();
            ship.BaseShip.BasePrice = 1000;
            // Blank upgrades
            ship.JumpDrive = new JumpDrive();
            ship.Weapon = new Weapon();
            ship.Shield = new Shield();
            ship.CosmoSystem = new CosmoSystem();

            // Assert
            Assert.That(ship.TradeInValue, Is.EqualTo(800), "Trade in value should equal 80% of the BaseShip value amount");
        }

        [Test]
        public void TradeInValueFromSystem()
        {
            // Arrange
            Ship ship = new Ship();
            ship.BaseShip = new BaseShip();
            ship.BaseShip.BasePrice = 1000;
            // Blank upgrades
            ship.JumpDrive = new JumpDrive();
            ship.Weapon = new Weapon();
            ship.Shield = new Shield();
            ship.CosmoSystem = new CosmoSystem();
            SystemShip systemShip = new SystemShip();
            systemShip.BaseShip = ship.BaseShip;
            systemShip.PriceMultiplier = 2.0;
            ship.CosmoSystem.SystemShips.Add(systemShip);

            // Assert
            Assert.That(ship.TradeInValue, Is.EqualTo(1600), "Trade in value should equal 80% of 2x BaseShip value amount");
        }

        private int player1Id;
        private int player2Id;
        private int player1AttackCount;
        private int player2AttackCount;

        public void AttackAtSameTimeThread()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            Player player1 = db.Players.Where(p => p.PlayerId == player1Id).Single();
            Player player2 = db.Players.Where(p => p.PlayerId == player2Id).Single();

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    player1.Ship.Attack(player2.Ship);
                    player1.Ship.InProgressCombat.Status = Combat.CombatStatus.ShipDestroyed;
                    player1AttackCount++;
                }
                catch (ArgumentException ex)
                {
                    Assert.That(ex.Message, Is.Not.Null, "Check for exception message");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.That(ex.Message, Is.Not.Null, "Check for exception message");
                }
            }
        }

        [Test]
        public void AttackAtSameTime()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();
            player1Id = player1.PlayerId;
            player2Id = player2.PlayerId;
            player1AttackCount = 0;
            player2AttackCount = 0;

            Thread t = new Thread(new ThreadStart(this.AttackAtSameTimeThread));
            t.Start();
            
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    player2.Ship.Attack(player1.Ship);
                    player1.Ship.InProgressCombat.Status = Combat.CombatStatus.ShipDestroyed;
                    player2AttackCount++;
                }
                catch (ArgumentException ex)
                {
                    // Good
                    Assert.That(ex.Message, Is.Not.Null, "Check for exception message");
                }
                catch (InvalidOperationException ex)
                {
                    // Good
                    Assert.That(ex.Message, Is.Not.Null, "Check for exception message");
                }
            }

            t.Join();
            Debug.WriteLine(string.Format("Attacked Player 1: {0} Player 2: {1}", player1AttackCount, player2AttackCount));
        }

        [Test]
        public void InProgressCombat()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();

            player1.Ship.Attack(player2.Ship);

            Assert.That(player1.Ship.InProgressCombat, Is.Not.Null, "Player 1 ship should be in combat");
            Assert.That(player2.Ship.InProgressCombat, Is.Not.Null, "Player 2 ship should be in combat");
        }

        [Test]
        public void InProgressCombatEnded()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();

            player1.Ship.Attack(player2.Ship);
            player1.Ship.InProgressCombat.Status = Combat.CombatStatus.ShipDestroyed;

            Assert.That(player1.Ship.InProgressCombat, Is.Null, "Player 1 ship should no longer be in combat");
            Assert.That(player2.Ship.InProgressCombat, Is.Null, "Player 2 ship should no longer be in combat");
        }

        [Test]
        public void InProgressCombatNull()
        {
            Player player1 = this.CreateTestPlayer();

            Assert.That(player1.Ship.InProgressCombat, Is.Null, "Starting ships shouldn't have any in-progress combats");
        }

        [Test]
        public void Attack()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();

            player1.Ship.Attack(player2.Ship);
            Combat combat = player1.Ship.InProgressCombat;
            int weaponFires = 0;
            int weaponMisses = 0;

            while (combat.Status == Combat.CombatStatus.Incomplete)
            {
                Debug.WriteLine(string.Format("Player 2 Shield {0} Hull: {1}", player2.Ship.DamageShield, player2.Ship.DamageHull));
                // Max out turn points for testing
                player1.Ship.InProgressCombat.TurnPointsLeft = 999;
                if (!player1.Ship.InProgressCombat.FireWeapon())
                {
                    weaponMisses++;
                }
                weaponFires++;
            }
            Debug.WriteLine(string.Format("Weapon Fires: {0} Misses: {1} Perc: {2}", weaponFires, weaponMisses, (100.0 / weaponFires * weaponMisses)));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "Target ship")]
        public void AttackTargetBusy()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();
            Player player3 = this.CreateTestPlayer();

            player3.Ship.Attack(player2.Ship);
            player1.Ship.Attack(player2.Ship);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "Current ship")]
        public void AttackSelfBusy()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();
            Player player3 = this.CreateTestPlayer();

            player1.Ship.Attack(player2.Ship);
            player1.Ship.Attack(player3.Ship);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "Current ship")]
        public void AttackBothBusy()
        {
            Player player1 = this.CreateTestPlayer();
            Player player2 = this.CreateTestPlayer();
            Player player3 = this.CreateTestPlayer();
            Player player4 = this.CreateTestPlayer();

            player1.Ship.Attack(player3.Ship);
            player2.Ship.Attack(player4.Ship);

            player1.Ship.Attack(player2.Ship);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "Cannot attack self")]
        public void AttackSelf()
        {
            Player player1 = this.CreateTestPlayer();

            player1.Ship.Attack(player1.Ship);
        }

        [Test]
        public void GetSystemDistance()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            Player testPlayer = this.CreateTestPlayer();
            CosmoSystem [] inRangeSystems = testPlayer.Ship.GetInRangeSystems();
            int range = testPlayer.Ship.JumpDrive.Range;

            // Check the distance of all the in-range systems
            foreach (CosmoSystem system in inRangeSystems)
            {
                double distance = testPlayer.Ship.GetSystemDistance(system);
                Assert.That(distance, Is.LessThan(range), "All in range systems should be within JumpDrive range");
            }
        }

        [Test]
        public void GetNearestBankSystem()
        {
            Player testPlayer = this.CreateTestPlayer();

            CosmoSystem bankSystem = testPlayer.Ship.GetNearestBankSystem();
            double bankDistance = testPlayer.Ship.GetSystemDistance(bankSystem);
            Debug.WriteLine(string.Format("Bank is {0} sectors away", bankDistance));

            Assert.That(bankSystem.HasBank, Is.True, "System should have a bank");
        }
    }
}
