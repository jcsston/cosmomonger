﻿//-----------------------------------------------------------------------
// <copyright file="BankController.cs" company="CosmoMonger">
//     Copyright (c) 2009 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Controllers
{
    using System;
    using System.Web.Mvc;
    using CosmoMonger.Models;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

    /// <summary>
    /// Controller for bank actions such as viewing balance, deposit, and withdraw.
    /// </summary>
    public class BankController : GameController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BankController"/> class.
        /// This is the default constructor that doesn't really to anything.
        /// </summary>
        public BankController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BankController"/> class.
        /// This constructor is used for unit testing purposes.
        /// </summary>
        /// <param name="manager">The game manager object to use.</param>
        public BankController(GameManager manager) 
            : base(manager)
        {
        }

        /// <summary>
        /// The Index action for the Bank controller.
        /// Redirects to the Bank action.
        /// </summary>
        /// <returns>Redirect to the Bank action</returns>
        public ActionResult Index()
        {
            return RedirectToAction("Bank");
        }

        /// <summary>
        /// The primary action for the Bank controller.
        /// Returns a View filled in with the current players bank stats.
        /// </summary>
        /// <returns>The Bank View</returns>
        public ActionResult Bank()
        {
            Player currentPlayer = this.ControllerGame.CurrentPlayer;
            ViewData["PlayerName"] = currentPlayer.Name;
            ViewData["CashCredits"] = currentPlayer.Ship.Credits;
            ViewData["BankCredits"] = currentPlayer.BankCredits;
            ViewData["BankAvailable"] = currentPlayer.Ship.CosmoSystem.HasBank;
            ViewData["SystemName"] = currentPlayer.Ship.CosmoSystem.Name;

            return View();
        }

        /// <summary>
        /// Deposits the specified number of credits into the bank.
        /// </summary>
        /// <param name="depositCredits">The number of credits to deposit.</param>
        /// <returns>A redirect to the Index action</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Deposit(int depositCredits)
        {
            try
            {
                this.ControllerGame.CurrentPlayer.BankDeposit(depositCredits);
                return RedirectToAction("Bank");
            }
            catch (InvalidOperationException ex)
            {
                // Log this exception
                ExceptionPolicy.HandleException(ex, "Controller Policy");

                ModelState.AddModelError("_FORM", ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Log this exception
                ExceptionPolicy.HandleException(ex, "Controller Policy");

                ModelState.AddModelError("depositCredits", ex.Message, depositCredits);
            }

            return View();
        }

        /// <summary>
        /// Withdraws the specified number of credits from the bank.
        /// </summary>
        /// <param name="withdrawCredits">The number of credits to withdraw.</param>
        /// <returns>A redirect to the Index action</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Withdraw(int withdrawCredits)
        {
            try
            {
                this.ControllerGame.CurrentPlayer.BankWithdraw(withdrawCredits);
                return RedirectToAction("Bank");
            }
            catch (InvalidOperationException ex)
            {
                // Log this exception
                ExceptionPolicy.HandleException(ex, "Controller Policy");

                ModelState.AddModelError("_FORM", ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Log this exception
                ExceptionPolicy.HandleException(ex, "Controller Policy");

                ModelState.AddModelError("withdrawCredits", ex.Message, withdrawCredits);
            }

            return View();
        }
    }
}
