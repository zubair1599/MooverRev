using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class Quote
    {
        internal void UpdateCachedItemValues()
        {
            this.CachedDuration = this.CalculateFurnitureTime();
            this.CachedWeight = this.CalculateWeight();
            this.CachedCubicFeet = this.CalculateCubicFeet();
        }

        /// <summary>
        /// Skips the cache and force-recalculates furniture time
        /// </summary>
        /// <returns></returns>
        private decimal CalculateFurnitureTime()
        {
            return this.GetStops().Sum(i => i.GetMoveTime()) / this.GetMinimumMoversRequired();
        }

        /// <summary>
        /// Skips cache and force-recalculates weight
        /// </summary>
        /// <returns></returns>
        private decimal CalculateWeight()
        {
            return this.GetStops().Sum(i => i.GetWeight());
        }

        /// <summary>
        /// Skips cache and force-recalculates cubic feet
        /// </summary>
        /// <returns></returns>
        private decimal CalculateCubicFeet()
        {
            return this.GetStops().Sum(i => i.GetCubicFeet());
        }

        /// <summary>
        /// Get cached weight if available, otherwise recalculate weight
        /// </summary>
        public decimal GetWeight()
        {
            if (this.CachedWeight.HasValue)
            {
                return this.CachedWeight.Value;
            }

            return this.CalculateWeight();
        }

        /// <summary>
        /// Estimated time to load and unload trucks -- gets cached if available
        /// </summary>
        /// <returns></returns>
        private decimal GetFurnitureTime()
        {
            if (this.CachedDuration.HasValue)
            {
                return this.CachedDuration.Value;
            }

            return this.CalculateFurnitureTime();
        }

        /// <summary>
        /// Get Cached cubic footage if available, otherwise recalculate
        /// </summary>
        public decimal GetCubicFeet()
        {
            if (this.CachedCubicFeet.HasValue)
            {
                return this.CachedCubicFeet.Value;
            }

            return this.CalculateCubicFeet();
        }
    }
}
