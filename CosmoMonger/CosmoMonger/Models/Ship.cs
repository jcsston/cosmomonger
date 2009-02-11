﻿//-----------------------------------------------------------------------
// <copyright file="Ship.cs" company="CosmoMonger">
//     Copyright (c) 2008 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Models
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
using System.Data.Linq;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

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

                // If the same ship is for sell in the current system, that price replaces shipValue
                SystemShip matchingShip = (from ss in this.CosmoSystem.SystemShips
                                           where ss.BaseShip == this.BaseShip
                                           select ss).SingleOrDefault();
                if (matchingShip != null)
                {
                    shipValue = matchingShip.Price;
                }

                // Take 20% off the face value of the ship to account for wear and tear
                return (int)(shipValue * 0.80);
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


            // This formula is use to determine the time to travel
            // 5 / ln(engine power + 0.5) = number of seconds to charge
            int travelTime = (int)(5 / Math.Log(this.JumpDrive.Speed + 0.5));

            // Update the database
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();
            this.TargetSystemId = targetSystem.SystemId;
            this.TargetSystemArrivalTime = DateTime.Now.AddSeconds(travelTime);
            db.SubmitChanges();

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
                if (this.TargetSystemArrivalTime < DateTime.Now)
                {
                    CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

                    // The ship has arrived, change the location of the ship and clear out the travel fields
                    this.CosmoSystem = (from s in db.CosmoSystems 
                                        where s.SystemId == this.TargetSystemId.Value
                                        select s).Single();
                    this.TargetSystemId = null;
                    this.TargetSystemArrivalTime = null;

                    // Send changes to the database
                    db.SubmitChanges();
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
        public virtual void Attack(Ship target)
        {
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();

            // Check if this ship is already in combat
            if (this.InProgressCombatsAttacker.Union(this.InProgressCombatsDefender).Any())
            {
                throw new InvalidOperationException("Current ship is already in combat");
            }

            // Check that the target ship is not in combat
            if (target.InProgressCombatsAttacker.Union(target.InProgressCombatsDefender).Any())
            {
                throw new ArgumentException("Target ship is already in combat", "target");
            }
            
            InProgressCombat combat = new InProgressCombat();
            combat.AttackerShip = this;
            combat.DefenderShip = target;

            // Save changes to the database
            db.InProgressCombats.InsertOnSubmit(combat);

            try
            {
                db.SubmitChanges();
            }
            catch (DuplicateKeyException ex)
            {
                ExceptionPolicy.HandleException(ex, "SQL Policy");

                // A combat must already be in-progress
                throw new InvalidOperationException("Ship is already in combat");
            }
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
    }
}
