﻿//-----------------------------------------------------------------------
// <copyright file="Ship.cs" company="CosmoMonger">
//     Copyright (c) 2008-2009 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// Extension of the partial LINQ class Ship
    /// </summary>
    public partial class Ship
    {
        /// <summary>
        /// Gets the current free cargo space for this ship.
        /// Calculated by taking the total cargo space and subtracting all upgrades, cargo, etc.
        /// </summary>
        public virtual int CargoSpaceFree
        {
            get
            {
                int cargoSpace = this.CargoSpaceTotal;
                cargoSpace -= this.ShipGoods.Sum(x => x.Quantity);
                cargoSpace -= this.JumpDrive.CargoCost;
                cargoSpace -= this.Shield.CargoCost;
                cargoSpace -= this.Weapon.CargoCost;
                return cargoSpace;
            }
        }
        
        /// <summary>
        /// Gets to total amount of cargo space 
        /// Currently this is just the BaseShip model CargoSpace value. 
        /// But we have this to account for possible future ship upgrades that actually upgrade your cargo space.
        /// </summary>
        public virtual int CargoSpaceTotal
        {
            get
            {
                return this.BaseShip.CargoSpace;
            }
        }

        /// <summary>
        /// Gets the current trade-in value for this ship.
        /// Calculated by looking in the current system and seeing what a matching ship is selling for, if that is not found then
        /// the BaseShip.BasePrice is taken.
        /// </summary>
        public virtual int TradeInValue
        {
            get
            {
                // Starting value is the base price
                int shipValue = this.BaseShip.BasePrice;

                // If the same ship is for sale in the current system, that price replaces shipValue
                SystemShip matchingShip = (from ss in this.CosmoSystem.SystemShips
                                           where ss.BaseShip == this.BaseShip
                                           select ss).SingleOrDefault();
                if (matchingShip != null)
                {
                    shipValue = matchingShip.Price;
                }

                // Add the trade-in-value of the upgrades
                shipValue += this.JumpDrive.GetTradeInValue(this) + this.Weapon.GetTradeInValue(this) + this.Shield.GetTradeInValue(this);

                // Take 20% off the face value of the ship to account for wear and tear
                return (int)(shipValue * 0.80);
            }
        }

        /// <summary>
        /// Gets the worth of the cargo aboard the ship.
        /// </summary>
        public virtual int CargoWorth
        {
            get
            {
                return this.ShipGoods.Sum(g => g.Quantity * g.Good.BasePrice);
            }
        }

        /// <summary>
        /// Gets the current in progress combat if any
        /// </summary>
        /// <value>The in progress combat object, null if no combat is taking place.</value>
        public virtual Combat InProgressCombat
        {
            get
            {
                /* 
                // This is a more optimized query, results in a single query to the database with Complete in the where clause
                CosmoMongerDbDataContext db = CosmoManager.GetDbContext();
                return (from c in db.Combats
                        where !c.Complete
                        && (c.AttackerShip == this || c.DefenderShip == this)
                        select c).SingleOrDefault();
                */

                // This query seems to pull in all combats the ship has ever been in, Complete is not part of the where clause
                return (from c in this.CombatsAttacker
                        where c.Status == Combat.CombatStatus.Incomplete
                        select c).Union(
                        (from c in this.CombatsDefender
                         where c.Status == Combat.CombatStatus.Incomplete
                        select c)).SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Ship"/> is destroyed.
        /// </summary>
        /// <value><c>true</c> if destroyed; otherwise, <c>false</c>.</value>
        public virtual bool Destroyed
        {
            get
            {
                return this.DamageHull >= 100;
            }
        }

        /// <summary>
        /// Gets the race of this ship, player or NPC.
        /// </summary>
        /// <value>The race of the ship.</value>
        public virtual Race Race
        {
            get
            {
                return (from p in this.Players
                        select p.Race)
                        .Union(
                        (from n in this.Npcs
                         select n.Race)).SingleOrDefault();
            }
        }

        /// <summary>
        /// Gets the 'name' of this ship, player name or NPC name.
        /// </summary>
        /// <value>The name of the ship.</value>
        public virtual string Name
        {
            get
            {
                return (from p in this.Players
                        select p.Name)
                        .Union(
                        (from n in this.Npcs
                         select n.Name)).SingleOrDefault();
            }
        }

        /// <summary>
        /// Starts the ship traveling to the target system.
        /// </summary>
        /// <param name="targetSystem">The target system to travel to.</param>
        /// <returns>The number of seconds before the ship arrives at the target system</returns>
        /// <exception cref="ArgumentException">Thrown if the ship is already in the target system</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the target system is out of range of the ship</exception>
        /// <exception cref="InvalidOperationException">Thrown if the ship is already traveling</exception>
        public virtual int Travel(CosmoSystem targetSystem)
        {
            // Check if ship is in the target system
            if (this.CosmoSystem == targetSystem)
            {
                throw new ArgumentException("Ship is already in the target system", "targetSystem");
            }

            // Check that the system is within range
            CosmoSystem[] inRangeSystems = this.GetInRangeSystems();
            if (!inRangeSystems.Contains(targetSystem))
            {
                throw new ArgumentOutOfRangeException("targetSystem", "Target system is out of JumpDrive range");
            }

            // Check that the ship is not already traveling
            if (this.TargetSystemId != null || this.TargetSystemArrivalTime != null)
            {
                throw new InvalidOperationException("Ship is already traveling");
            }

            // Get the travel time
            int travelTime = this.JumpDrive.ChargeTime;

            // Update the player stats
            Player shipPlayer = this.Players.SingleOrDefault();
            if (shipPlayer != null)
            {
                shipPlayer.DistanceTraveled += this.GetSystemDistance(targetSystem);
            }

            // Update the database
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();
            this.TargetSystemId = targetSystem.SystemId;
            this.TargetSystemArrivalTime = DateTime.UtcNow.AddSeconds(travelTime);
            db.SaveChanges();

            return travelTime;
        }

        /// <summary>
        /// Checks if ship is currently traveling.
        /// </summary>
        /// <returns>true if traveling, false if no longer traveling</returns>
        public virtual bool CheckIfTraveling()
        {
            // Do we have an arrival time?
            if (this.TargetSystemArrivalTime != null)
            {
                // Assert that there also is a target system id (should never happen)
                Debug.Assert(this.TargetSystemId.HasValue, "There also should be a target system");

                // Has the arrival time passed?
                if (this.TargetSystemArrivalTime < DateTime.UtcNow)
                {
                    CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

                    // The ship has arrived, change the location of the ship and clear out the travel fields
                    this.CosmoSystem = (from s in db.CosmoSystems 
                                        where s.SystemId == this.TargetSystemId.Value
                                        select s).Single();
                    this.TargetSystemId = null;
                    this.TargetSystemArrivalTime = null;

                    db.SaveChanges();
                }
                else
                {
                    // Ship is still traveling
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a list of Systems within traveling range of the Ship. Excluding the current system.
        /// </summary>
        /// <returns>Array of CosmoSystems within JumpDrive distance</returns>
        public virtual CosmoSystem[] GetInRangeSystems()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();
            
            // Find all systems within range of the JumpDrive
            // We use the distance formula, sqrt((x2 - x1)^2 + (y2 - y1)^2)
            var systems = (from s in db.CosmoSystems
                           where s != this.CosmoSystem
                           && Math.Sqrt(Math.Pow(this.CosmoSystem.PositionX - s.PositionX, 2) 
                                + Math.Pow(this.CosmoSystem.PositionY - s.PositionY, 2))
                              < this.JumpDrive.Range
                           select s);

            return systems.ToArray();
        }

        /// <summary>
        /// Gets the goods on this ship
        /// </summary>
        /// <returns>Array of ShipGoods</returns>
        public virtual ShipGood[] GetGoods()
        {
            return this.ShipGoods.ToArray();
        }

        /// <summary>
        /// Fetches the ShipGood object for the passed in goodId id. 
        /// </summary>
        /// <param name="goodId">The good id of the ShipGood object to get.</param>
        /// <returns>
        /// The ShipGood object with the matching goodId. 
        /// If there is no ShipGood for the passed in good id, null is returned.
        /// </returns>
        public virtual ShipGood GetGood(int goodId)
        {
            return (from g in this.ShipGoods where g.GoodId == goodId select g).SingleOrDefault();
        }

        /// <summary>
        /// Attacks the target ship.
        /// </summary>
        /// <param name="target">The target ship to attack.</param>
        /// <exception cref="InvalidOperationException">Thrown when this ship is already in combat</exception>
        /// <exception cref="ArgumentException">Thrown when target ship is already in combat</exception>
        /// <exception cref="ArgumentException">Thrown when trying to attack self</exception>
        public virtual void Attack(Ship target)
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Check if this ship is already in combat
            if (this.InProgressCombat != null)
            {
                throw new InvalidOperationException("Current ship is already in combat");
            }

            // Check that the target ship is not in combat
            if (target.InProgressCombat != null)
            {
                throw new ArgumentException("Target ship is already in combat", "target");
            }

            // Check that we are not trying to attack ourself...
            if (target == this)
            {
                throw new ArgumentException("Cannot attack self", "traget");
            }

            Combat combat = new Combat();
            combat.AttackerShip = this;
            combat.DefenderShip = target;
            combat.TurnPointsLeft = Combat.PointsPerTurn;
            combat.Status = Combat.CombatStatus.Incomplete;
            combat.Surrendered = false;
            combat.Turn = 0;
            combat.CargoJettisoned = false;
            combat.Search = false;
            combat.LastActionTime = DateTime.UtcNow;

            // Save changes to the database
            db.Combats.InsertOnSubmit(combat);

            try
            {
                db.SaveChanges();
            }
            catch (SqlException ex)
            {
                ExceptionPolicy.HandleException(ex, "SQL Policy");

                // A combat must already be in-progress
                // Remove this row
                db.Combats.DeleteOnSubmit(combat);

                throw new InvalidOperationException("Ship is already in combat");
            }
            catch (DuplicateKeyException ex)
            {
                ExceptionPolicy.HandleException(ex, "SQL Policy");

                // A combat must already be in-progress
                throw new InvalidOperationException("Ship is already in combat");
            }
        }

        /// <summary>
        /// Gets ships that are leaving the system and we can attack
        /// </summary>
        /// <returns>An array of Ship objects that are leaving the system and open to attack</returns>
        public virtual IEnumerable<Ship> GetShipsToAttack()
        {
            return (from s in this.CosmoSystem.GetLeavingShips()
                    where s != this
                    select s).AsEnumerable();
        }

        /// <summary>
        /// Gets the other ships in system.
        /// </summary>
        /// <returns>An array of Ship objects that are currently in the system, (includes ones we can attack)</returns>
        public virtual IEnumerable<Ship> GetOtherShipsInSystem()
        {
            return (from s in this.CosmoSystem.GetShipsInSystem()
                    where s != this
                    select s).AsEnumerable();
        }

        /// <summary>
        /// Add the good to this ship.
        /// </summary>
        /// <param name="goodId">The good type to add to this ship.</param>
        /// <param name="quantity">The quantity of the good to add.</param>
        /// <returns>The number of goods actually added to the ship. 0 is returned when ship cargo is full.</returns>
        public virtual int AddGood(int goodId, int quantity)
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Limit the quantity to the amount of free cargo space
            int actualQuantity = Math.Min(quantity, this.CargoSpaceFree);

            Dictionary<string, object> props = new Dictionary<string, object>
            {
                { "GoodId", goodId },
                { "Quantity", quantity },
                { "ActualQuantity", actualQuantity },
                { "ShipId", this.ShipId }
            };
            Logger.Write("Adding Good to Ship in Ship.AddGood", "Model", 150, 0, TraceEventType.Verbose, "Adding Good to Ship", props);

            ShipGood shipGood = this.GetGood(goodId);
            if (shipGood == null)
            {
                // Ship is not already carrying this good, so we have to create a new ShipGood
                shipGood = new ShipGood();
                shipGood.Ship = this;
                shipGood.GoodId = goodId;
                shipGood.Quantity = actualQuantity;

                db.ShipGoods.InsertOnSubmit(shipGood);
            }
            else
            {
                // Add the correct number of goods to the ship
                shipGood.Quantity += actualQuantity;
            }

            db.SaveChanges();

            // Return the number of goods added to the ship
            return actualQuantity;
        }

        /// <summary>
        /// Gets the distance from the ship to the target system
        /// </summary>
        /// <param name="targetSystem">The target system to measure to.</param>
        /// <returns>A floating-point distance</returns>
        public double GetSystemDistance(CosmoSystem targetSystem)
        {
            return Math.Sqrt(Math.Pow(targetSystem.PositionX - this.CosmoSystem.PositionX, 2) + Math.Pow(targetSystem.PositionY - this.CosmoSystem.PositionY, 2));
        }

        /// <summary>
        /// Gets the nearest system with a bank. May return the system the ship currently is in.
        /// </summary>
        /// <returns>The a reference to the nearest system with a bank</returns>
        public CosmoSystem GetNearestBankSystem()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            var systemsWithBank = (from s in db.CosmoSystems
                                   where s.HasBank
                                   orderby Math.Sqrt(Math.Pow(this.CosmoSystem.PositionX - s.PositionX, 2) 
                                            + Math.Pow(this.CosmoSystem.PositionY - s.PositionY, 2))
                                   select s);
            return systemsWithBank.First();
        }

        /// <summary>
        /// Completely repairs this ship.
        /// </summary>
        public void Repair()
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            this.DamageEngine = 0;
            this.DamageHull = 0;
            this.DamageShield = 0;
            this.DamageWeapon = 0;

            db.SaveChanges();
        }

        /// <summary>
        /// Applies the damage of a weapon hit to the ship.
        /// </summary>
        /// <remarks>Does not apply changes to database, must call SubmitChanges to save.</remarks>
        /// <param name="weaponDamage">The full weapon damage amount.</param>
        public void ApplyDamage(double weaponDamage)
        {
            // Grab ship base shield strength
            double shieldPower = this.Shield.Strength;

            Race shipRace = this.Race;
            if (shipRace != null)
            {
                // Apply racial factor to shields -/+ 10%
                shieldPower *= 1.0 + (shipRace.Shields / 10.0);
            }

            // Formula is: ShieldStrength * (1 - (ShieldDamage / 100))
            // Which means 0% damage gives full power, 50% damage gives half power, 
            // and 100% damage gives no shield power
            shieldPower *= 1.0 - (this.DamageShield / 100.0);

            // Reduce the weapon damage, depending on shield power
            weaponDamage = Math.Max(weaponDamage - shieldPower, 1.0);

            // 50% of the damage goes to shields and rest goes to the hull
            double newDamageShield = this.DamageShield + (weaponDamage / 0.5);
            newDamageShield = Math.Ceiling(newDamageShield);

            // Hull strength is dependant on the ship level
            // Level 1 is base, level 2 +25% strength, level 3 +50%, level 4 +75%, level 5 +100%
            double hullStrength = 1.0 + (this.BaseShip.Level - 1) / 4.0;

            double damageToHull = ((weaponDamage / 1.5) * (this.DamageShield / 100.0)) / hullStrength;
            double newDamageHull = Math.Ceiling(this.DamageHull + damageToHull);

            Dictionary<string, object> props = new Dictionary<string, object>
            {
                { "ShipId", this.ShipId },
                { "WeaponDamage", weaponDamage },
                { "DamageShield", this.DamageShield },
                { "DamageHull", this.DamageHull },
                { "HullStrength", hullStrength },
                { "NewDamageShield", newDamageShield },
                { "NewDamageHull", newDamageHull }
            };
            Logger.Write("Applyed damage to ship", "Model", 150, 0, TraceEventType.Verbose, "Ship.ApplyDamage", props);

            // Max damage is 100%
            this.DamageShield = (int)Math.Min(newDamageShield, 100);
            this.DamageHull = (int)Math.Min(newDamageHull, 100);
        }

        /// <summary>
        /// Called when the BaseShip/BaseShipId is changed.
        /// </summary>
        partial void OnBaseShipIdChanged()
        {
            Player player = this.Players.SingleOrDefault();
            if (player != null)
            {
                player.UpdateNetWorth();
            }
        }

        /// <summary>
        /// Called when the JumpDrive/JumpDriveId is changed.
        /// </summary>
        partial void OnJumpDriveIdChanged()
        {
            Player player = this.Players.SingleOrDefault();
            if (player != null)
            {
                player.UpdateNetWorth();
            }
        }

        /// <summary>
        /// Called when the Shield/ShieldId is changed.
        /// </summary>
        partial void OnShieldIdChanged()
        {
            Player player = this.Players.SingleOrDefault();
            if (player != null)
            {
                player.UpdateNetWorth();
            }
        }

        /// <summary>
        /// Called when the Weapon/WeaponId is changed.
        /// </summary>
        partial void OnWeaponIdChanged()
        {
            Player player = this.Players.SingleOrDefault();
            if (player != null)
            {
                player.UpdateNetWorth();
            }
        }

        /// <summary>
        /// A property changed event, called when Credits is changed.
        /// </summary>
        partial void OnCreditsChanged()
        {
            // Because Credits has changed we need to update NetWorth for players
            Player player = this.Players.SingleOrDefault();
            if (player != null)
            {
                player.UpdateNetWorth();
            }
        }
    }
}
