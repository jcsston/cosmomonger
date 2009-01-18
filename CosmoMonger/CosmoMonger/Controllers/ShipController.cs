﻿//-----------------------------------------------------------------------
// <copyright file="ShipController.cs" company="CosmoMonger">
//     Copyright (c) 2008-2009 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using CosmoMonger.Models;

    /// <summary>
    /// This controller deals with all ship related tasks such as, 
    /// * Buying a ship
    /// * Buying ship upgrades
    /// * Selling ship upgrades, 
    /// * Viewing current ship information
    /// </summary>
    public class ShipController : GameController
    {
        /// <summary>
        /// Gets the current players ship and pass it to the ViewShip view.
        /// </summary>
        /// <returns>The ViewShip view filled in with the model data for the players current ship</returns>
        public ActionResult Index()
        {
            ViewData["Ship"] = this.ControllerGame.CurrentPlayer.Ship;
            return View("ViewShip");
        }

        /// <summary>
        /// This action will fetch the avaiable ships in the current players system 
        /// via the System.GetAvailableShips method and pass the data to the BuyShip view.
        /// </summary>
        /// <returns>The BuyShip view filled with the ships available for purchase</returns>
        public ActionResult BuyShip()
        {
            ViewData["Ships"] = this.ControllerGame.CurrentPlayer.Ship.CosmoSystem.GetAvailableShips();
            return View("BuyShip");
        }

        /// <summary>
        /// This action takes in the passed systemShip id and buys the ship via the SystemShip.Buy method. 
        /// Then redirects to the Index action.
        /// </summary>
        /// <param name="systemShipId">The system ship id to buy.</param>
        /// <returns>Redirect back to the Index action</returns>
        public ActionResult BuyShip(int systemShipId)
        {
            SystemShip shipToBuy = this.ControllerGame.CurrentPlayer.Ship.CosmoSystem.GetShip(systemShipId);
            if (shipToBuy != null)
            {
                shipToBuy.Buy(this.ControllerGame);
            }

            return RedirectToAction("Index");
        }

        /* TODO: Implement following actions in RC4 */
        
        /// <summary>
        /// This action will fetch the available ship engine upgrades via the System.GetEngineUpgrades method
        /// and pass the data to the BuyEngineUpgrade view.
        /// </summary>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyEngineUpgrade()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This action takes in the passed upgrade id,  buys the ship upgrade via the SystemEngineUpgrade.Buy method 
        /// and then redirects to the Index action.
        /// </summary>
        /// <param name="upgradeId">The upgrade id.</param>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyEngineUpgrade(int upgradeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This action will fetch the available ship shield upgrades via the System.GetShieldUpgrades method 
        /// and pass the data to the BuyShieldUpgrade view.
        /// </summary>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyShieldUpgrade()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This action takes in the passed upgrade id,  buys the ship upgrade via the SystemShieldUpgrade.Buy method, 
        /// and then redirects to the Index action.
        /// </summary>
        /// <param name="upgradeId">The upgrade id.</param>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyShieldUpgrade(int upgradeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This action will fetch the available ship weapon upgrades via the System.GetWeaponUpgrades method 
        /// and pass the data to the BuyWeaponUpgrade view.
        /// </summary>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyWeaponUpgrade()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This action takes in the passed upgrade id,  buys the ship upgrade via the SystemWeaponUpgrade.Buy method, 
        /// and then redirects to the Index action.
        /// </summary>
        /// <param name="upgradeId">The upgrade id.</param>
        /// <returns>Throws NotImplementedException</returns>
        public ActionResult BuyWeaponUpgrade(int upgradeId)
        {
            throw new NotImplementedException();
        }
    }
}
