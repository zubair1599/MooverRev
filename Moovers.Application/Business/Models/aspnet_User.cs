using System;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Business.Models
{
    using System.Collections.Generic;

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "*", Justification = "Risky to change manually")]
    public partial class aspnet_User
    {
        public bool HasPermissionsOn(Guid franchiseID)
        {
            return this.Franchise_aspnetUser.Any(i => i.FranchiseID == franchiseID);
        }

        public bool HasMultipleFranchises()
        {
            return this.Franchise_aspnetUser.Count() > 1;
        }

        private Franchise _singleFranchise;

        public Franchise GetSingleFranchise()
        {
            if (_singleFranchise == null)
            {
                _singleFranchise = this.Franchise_aspnetUser.Single().Franchise;
            }

            return _singleFranchise;
        }

        public IEnumerable<Franchise> GetAllFranchises()
        {
            return this.Franchise_aspnetUser.Select(fra => fra.Franchise).ToList();
        }
        
        public EmployeeAuth_Session CurrentSession { get; set; }
    }
}
