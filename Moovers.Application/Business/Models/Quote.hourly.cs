using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Enums;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class Quote
    {
        public decimal GetHourlyTruckMultiplier()
        {
            if (this.CustomHourlyRate.HasValue)
            {
                return this.CustomHourlyRate.Value;
            }

            var repo = new GlobalSettingRepository(this.FranchiseID);
            return repo.GetValue<decimal>(SettingTypes.HourlyTruck);
        }

        public decimal GetHourlyPersonMultiplier()
        {
            if (this.CustomHourlyRate.HasValue)
            {
                return 0m;
            }

            var repo = new GlobalSettingRepository(this.FranchiseID);
            return repo.GetValue<decimal>(SettingTypes.HourlyMan);
        }

        public decimal GetHourlyTruckDestinationMultiplier()
        {
            var repo = new GlobalSettingRepository(this.FranchiseID);
            return repo.GetValue<decimal>(SettingTypes.HourlyTruckDestination);
        }

        public decimal GetHourlyPersonDestinationMultiplier()
        {
            var repo = new GlobalSettingRepository(this.FranchiseID);
            return repo.GetValue<decimal>(SettingTypes.HourlyManDestination);
        }
    }
}
