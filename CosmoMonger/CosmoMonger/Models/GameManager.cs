//-----------------------------------------------------------------------
// <copyright file="GameManager.cs" company="CosmoMonger">
//     Copyright (c) 2008 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;    
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// This is the central control class for the CosmoMonger game.
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        public User CurrentUser
        {
            get
            {
                return this.CurrentPlayer.User;
            }
        }

        /// <summary>
        /// Gets the current player
        /// </summary>
        /// <value>The current player.</value>
        public Player CurrentPlayer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the CosmoMonger db context.
        /// </summary>
        /// <returns>LINQ CosmoMongerDbDataContext object</returns>
        public static CosmoMongerDbDataContext GetDbContext()
        {
            return new CosmoMongerDbDataContext();
        }

        /// <summary>
        /// Calls DoAction on all NPCs in the galaxy. This method will be called every 5 seconds via Cache Expirations to keep the NPCs busy in the galaxy even when no human players are hitting pages.
        /// </summary>
        public void DoPendingNPCActions()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the user with the passed in id. Returns null if the user does not exist.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>User object, null if the user does not exist.</returns>
        public User GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetches the top records of players based on the recordType argument. If the recordType is invalid an ArgumentException is thrown.
        /// </summary>
        /// <param name="recordType">Type of the record.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>Array of Player objects</returns>
        public Player[] GetTopPlayers(string recordType, int limit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetches the Player object for the passed in player id. If the player doesn't exist, null is returned.
        /// </summary>
        /// <param name="playerId">The player id.</param>
        /// <returns>Player object, null if the player does not exist.</returns>
        public Player GetPlayer(int playerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return an array of all systems in the galaxy.
        /// </summary>
        /// <returns>Array of CosmoSystem objects</returns>
        public CosmoSystem[] GetSystems()
        {
            throw new NotImplementedException();
        }
    }
}