﻿//-----------------------------------------------------------------------
// <copyright file="SystemGood.cs" company="CosmoMonger">
//     Copyright (c) 2008-2009 CosmoMonger. All rights reserved.
// </copyright>
// <author>Jory Stone</author>
// <author>Roger Boykin</author>
//-----------------------------------------------------------------------
namespace CosmoMonger.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Linq;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    /// <summary>
    /// Extension of the partial LINQ class SystemGood
    /// </summary>
    public partial class SystemGood
    {
        /// <summary>
        /// Enum for the different types of Demand goods can have
        /// </summary>
        public enum DemandType
        {
            /// <summary>
            /// This good is not bought or sold at this system
            /// </summary>
            Prohibited = 0,

            /// <summary>
            /// The demand for this good is average
            /// </summary>
            Average = 1,

            /// <summary>
            /// The demand for this good is higher than average
            /// </summary>
            Increased = 2,

            /// <summary>
            /// The demand for this good is lower than average
            /// </summary>
            Decreased = 3
        }
        
        /// <summary>
        /// Gets the actual price of the good.
        /// Calcuated by taking the Good.BasePrice and the SystemGood.PriceMultiplier
        /// </summary>
        public virtual int Price
        {
            get
            {
                return (int)(this.Good.BasePrice * this.PriceMultiplier);
            }
        }

        /// <summary>
        /// Buys the specified quantity of goods from the system.
        /// </summary>
        /// <param name="currentShip">The Ship object to use for this transaction.</param>
        /// <param name="quantity">The quantity of the good to buy.</param>
        /// <param name="price">The price to buy the good at.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown on quantity param when trying to buy more goods than avaiable in the system.
        /// Thrown on price param when asking price is different than the actual current price.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when there is not enough credits or cargo space to buy the requested number of goods.</exception>
        public virtual void Buy(Ship currentShip, int quantity, int price)
        {
            // Check that we are not trying to buy more goods than there is
            if (this.Quantity < quantity)
            {
                throw new ArgumentOutOfRangeException("quantity", quantity, "Unable to buy more goods than at the system");
            }

            if (this.Price != price)
            {
                throw new ArgumentOutOfRangeException("price", price, "Asking price does not match current price");
            }

            // Check if the ship has enough money to buy the goods
            int totalCost = (int)this.Price * quantity;
            if (currentShip.Credits < totalCost)
            {
                throw new ArgumentException("Not enough credits to buy requested number of goods", "quantity");
            }
            
            // Check if the ship has enough cargo space to carry the goods
            if (currentShip.CargoSpaceFree < quantity)
            {
                throw new ArgumentException("Not enough cargo space to carry requested number of goods", "quantity");
            }

            Dictionary<string, object> props = new Dictionary<string, object>
            {
                { "ShipId", currentShip.ShipId },
                { "SystemId", this.SystemId },
                { "GoodId", this.GoodId },
                { "Quantity", quantity },
                { "Price", price },
                { "TotalCost", totalCost }
            };
            Logger.Write("Buying goods in SystemGood.Buy", "Model", 500, 0, TraceEventType.Information, "Buying Goods", props);

            // Remove the goods from the system
            this.Quantity -= quantity;

            // We don't delete the SystemGood even if quantity is 0 because the SystemGood also signals if the system buys the good
            CosmoMongerDbDataContext db = CosmoManager.GetDbContext();
            
            // Commit changes to the database
            db.SaveChanges();

            // Add the goods to the player ship
            int addedQuantity = currentShip.AddGood(this.GoodId, quantity);
            
            // We should have checked that the ship had enough space to hold the good already
            Debug.Assert(addedQuantity == quantity, "The ship should have enough space to hold all the goods");

            // Charge the ship for the goods
            currentShip.Credits -= totalCost;

            // Commit changes to the database
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the price history for this system good
        /// </summary>
        /// <returns>Dictionary of DateTimes and the price at that point in time.</returns>
        public virtual Dictionary<DateTime, int> GetPriceHistory()
        {
            Dictionary<DateTime, int> priceHistory = new Dictionary<DateTime, int>();
            Regex priceMultiplierRegex = new Regex("PriceMultiplier: (\\d+.\\d+)");
            SqlConnection logConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoggingConnectionString"].ConnectionString);
            logConnection.Open();
            using (logConnection)
            {
                SqlCommand priceHistoryCmd = logConnection.CreateCommand();
                priceHistoryCmd.CommandText = "SELECT Timestamp, FormattedMessage FROM Log WHERE Title = 'Adjusting Good Price' AND FormattedMessage LIKE '%SystemId: " + this.SystemId + "\r%' AND FormattedMessage LIKE '%GoodId: " + this.GoodId + "\r%' ORDER BY Timestamp";
                priceHistoryCmd.CommandTimeout = 600;

                SqlDataReader reader = priceHistoryCmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime timestamp = (DateTime)reader["Timestamp"];
                    string formattedMessage = reader["FormattedMessage"] as string;
                    if (formattedMessage != null)
                    {
                        Match match = priceMultiplierRegex.Match(formattedMessage);
                        if (match != null && match.Success)
                        {
                            double priceMultipler = double.Parse(match.Groups[1].Value);
                            int price = (int)(priceMultipler * this.Good.BasePrice);
                            priceHistory[timestamp] = price;
                        }
                    }
                }

                reader.Close();
            }

            return priceHistory;
        }
    }
}
