﻿namespace CosmoMonger.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using CosmoMonger.Models;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// This controller handles all combat related tasks such as selecting a ship to attack,
    /// taking combat turns, and victory/defeat conditions.
    /// </summary>
    public class CombatController : GameController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Attack");
        }

        public ActionResult Attack()
        {
            Ship playerShip = this.ControllerGame.CurrentPlayer.Ship;
            // Check if there is a combat in progress
            if (playerShip.InProgressCombat != null)
            {
                // Redirect to combat start
                return RedirectToAction("CombatStart");
            }

            ViewData["Ships"] = playerShip.CosmoSystem.GetShipsInSystem().Where(s => s != playerShip);
            ViewData["ShipsToAttack"] = playerShip.GetShipsToAttack();

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Attack(int shipId)
        {
            Ship shipToAttack = this.ControllerGame.GetShip(shipId);
            if (shipToAttack != null)
            {
                try
                {
                    this.ControllerGame.CurrentPlayer.Ship.Attack(shipToAttack);
                    return RedirectToAction("CombatStart");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("shipId", ex);
                }
            }
            else
            {
                ModelState.AddModelError("shipId", "Invalid ship");
            }
            
            return View();
        }

        public ActionResult CombatStart()
        {
            Combat currentCombat = this.ControllerGame.CurrentPlayer.Ship.InProgressCombat;
            if (currentCombat != null)
            {
                ViewData["Combat"] = currentCombat;
                ViewData["PlayerName"] = this.ControllerGame.CurrentPlayer.Name;
                ViewData["PlayerShip"] = this.ControllerGame.CurrentPlayer.Ship;
                if (this.ControllerGame.CurrentPlayer.Ship == currentCombat.AttackerShip)
                {
                    ViewData["EnemyName"] = currentCombat.DefenderShip.Players.Single().Name;
                    ViewData["EnemyShip"] = currentCombat.DefenderShip;
                }
                else
                {
                    ViewData["EnemyName"] = currentCombat.AttackerShip.Players.Single().Name;
                    ViewData["EnemyShip"] = currentCombat.AttackerShip;
                }

                return View();
            }
            else
            {
                return View("CombatNone");
            }
        }

        public ActionResult CombatComplete(int combatId)
        {
            Combat combat = this.ControllerGame.GetCombat(combatId);
            if (combat != null)
            {
                ViewData["CreditsLooted"] = combat.CreditsLooted;

                Ship playerShip = this.ControllerGame.CurrentPlayer.Ship;
                switch (combat.Status)
                {
                    case Combat.CombatStatus.ShipDestroyed:
                        if (combat.ShipTurn == playerShip)
                        {
                            ViewData["Message"] = "You have won";
                            ViewData["CargoLooted"] = combat.CombatGoods;
                        }
                        else
                        {
                            ViewData["Message"] = "You have lost";
                            ViewData["CargoLost"] = combat.CombatGoods;
                        }
                        break;

                    case Combat.CombatStatus.CargoPickup:
                        if (combat.ShipTurn == playerShip)
                        {
                            ViewData["Message"] = "Picked up jettisioned cargo";
                            ViewData["CargoLooted"] = combat.CombatGoods;
                        }
                        else
                        {
                            ViewData["Message"] = "You escaped by jettisoning cargo";
                            ViewData["CargoLost"] = combat.CombatGoods;
                        }
                        break;

                    case Combat.CombatStatus.ShipFled:
                        if (combat.ShipTurn == playerShip)
                        {
                            ViewData["Message"] = "You have escaped";
                            ViewData["FinalImage"] = "RunningChicken.png";
                        }
                        else
                        {
                            ViewData["Message"] = "Enemy has escaped";
                        }
                        break;

                    case Combat.CombatStatus.ShipSurrendered:
                        if (combat.ShipTurn == playerShip)
                        {
                            ViewData["Message"] = "You have captured the opposing player";
                            ViewData["CargoLooted"] = combat.CombatGoods;
                        }
                        else
                        {
                            ViewData["Message"] = "You have surrendered";
                            ViewData["CargoLost"] = combat.CombatGoods;
                        }
                        break;
                }

                return View();
            }
            else
            {
                return View("NoCombat");
            }
        }

        /// <summary>
        /// Check if the ship has entered combat
        /// </summary>
        /// <returns>A JSON data object with combat fields.</returns>
        public JsonResult CombatPending()
        {
            Ship currentShip = this.ControllerGame.CurrentPlayer.Ship;
            bool inCombat = (currentShip.InProgressCombat != null);

            return Json(new { combat = inCombat });
        }

        public JsonResult CombatStatus(int combatId)
        {
            Combat selectedCombat = this.ControllerGame.GetCombat(combatId);
            if (selectedCombat != null)
            {
                return Json(BuildCombatStatus(selectedCombat));
            }

            return Json(false);
        }

        public JsonResult FireWeapon(int combatId)
        {
            Combat selectedCombat = this.ControllerGame.GetCombat(combatId);
            if (selectedCombat != null)
            {
                string message = null;

                // Check that it is the current players turn
                if (selectedCombat.ShipTurn == this.ControllerGame.CurrentPlayer.Ship)
                {
                    try
                    {
                        selectedCombat.FireWeapon();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Combat is over
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        // Not enough turn points
                        message = ex.Message;
                    }
                }

                return Json(new { message = message, status = BuildCombatStatus(selectedCombat) });
            }

            return Json(false);
        }

        public JsonResult ChargeJumpDrive(int combatId)
        {
            Combat selectedCombat = this.ControllerGame.GetCombat(combatId);
            if (selectedCombat != null)
            {
                string message = null;

                // Check that it is the current players turn
                if (selectedCombat.ShipTurn == this.ControllerGame.CurrentPlayer.Ship)
                {
                    try
                    {
                        selectedCombat.ChargeJumpDrive();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Combat is over
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        // Not enough turn points
                        message = ex.Message;
                    }
                }

                return Json(new { message = message, status = BuildCombatStatus(selectedCombat) });
            }

            return Json(false);
        }

        public JsonResult JettisonCargo(int combatId)
        {
            Combat selectedCombat = this.ControllerGame.GetCombat(combatId);
            if (selectedCombat != null)
            {
                string message = null;

                // Check that it is the current players turn
                if (selectedCombat.ShipTurn == this.ControllerGame.CurrentPlayer.Ship)
                {
                    try
                    {
                        selectedCombat.JettisonCargo();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Combat is over
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        // Not enough turn points
                        message = ex.Message;
                    }
                }

                return Json(new { message = message, status = BuildCombatStatus(selectedCombat) });
            }

            return Json(false);
        }

        public JsonResult PickupCargo(int combatId)
        {
            Combat selectedCombat = this.ControllerGame.GetCombat(combatId);
            if (selectedCombat != null)
            {
                string message = null;

                // Check that it is the current players turn
                if (selectedCombat.ShipTurn == this.ControllerGame.CurrentPlayer.Ship)
                {
                    try
                    {
                        selectedCombat.PickupCargo();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Combat is over
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        // Not enough turn points
                        message = ex.Message;
                    }
                }

                return Json(new { message = message, status = BuildCombatStatus(selectedCombat) });
            }

            return Json(false);
        }

        /// <summary>
        /// Builds a combat status object for JSON
        /// </summary>
        /// <param name="combat">The combat object to build from.</param>
        /// <returns>
        /// A combat status data object, suitable for JSON
        /// Fields:
        /// int playerHull - amount of damage to hull
        /// int playerShield
        /// int enemyHull - amount of damage to hull
        /// int enemyShield
        /// bool turn - True is player's turn, false is other players turn
        /// bool surrendered - When True the other player has offered a surrender
        /// bool cargoJettisoned - When True the other player has jettisoned their cargo
        /// int jumpDriveCharge - charge of JumpDrive
        /// int turnPoints - Number of turn points left
        /// bool complete - true when combat is complete
        /// </returns>
        private object BuildCombatStatus(Combat combat)
        {
            Ship playerShip, enemyShip;
            if (this.ControllerGame.CurrentPlayer.Ship == combat.AttackerShip)
            {
                playerShip = combat.AttackerShip;
                enemyShip = combat.DefenderShip;
            }
            else
            {
                playerShip = combat.DefenderShip;
                enemyShip = combat.AttackerShip;
            }

            return new { 
                playerHull = playerShip.DamageHull, 
                playerShield = playerShip.DamageShield,
                enemyHull = enemyShip.DamageHull,
                enemyShield = enemyShip.DamageShield,
                turn = (playerShip == combat.ShipTurn),
                surrendered = combat.Surrendered,
                cargoJettisoned = combat.CargoJettisoned,
                jumpDriveCharge = playerShip.CurrentJumpDriveCharge,
                turnPoints = combat.TurnPointsLeft,
                complete = (combat.Status != Combat.CombatStatus.Incomplete)
            };
        }
    }
}
