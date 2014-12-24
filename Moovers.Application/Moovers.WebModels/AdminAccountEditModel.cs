using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web;
using Business.Models;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    public class AccountEditModel
    {
        public const string UnchangedPasswordText = "[[!#--UnChAnGEd--#!]]";

        public Business.Models.aspnet_User User { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public IEnumerable<string> UserRoles { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Email { get; set; }

        public List<Guid> FranchiseIds { get; set; }

        public List<Franchise> SelectedFranchises { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Title { get; set; }

        public Guid EmployeeID { get; set; }

        public IEnumerable<Employee> Employees { get; set; }

        public IEnumerable<Franchise> GetFranchises()
        {
            var repo = new FranchiseRepository();
            return repo.GetAll();
        }
        

        public bool IsEdit
        {
            get { return this.User != null && this.User.UserId != Guid.Empty; }
        }

        public bool EmployeeEdit { get; set; }
        
        public AccountEditModel()
        {
            this.User = null;
            this.Roles = System.Web.Security.Roles.GetRolesForUser();

        }

        public AccountEditModel(Business.Models.aspnet_User user)
            : this()
        {
            var franchiserepo = new Franchise_aspnet_UserRepository();
            this.FranchiseIds = franchiserepo.GetAllUserFranchises(user.UserId);

            this.User = user;
            this.Email = user.aspnet_Membership.Email;
            this.FirstName = user.aspnet_Users_Profile.FirstOrDefault()!=null? user.aspnet_Users_Profile.FirstOrDefault().FirstName : string.Empty;
            this.LastName = user.aspnet_Users_Profile.FirstOrDefault() != null ? user.aspnet_Users_Profile.FirstOrDefault().LastName : string.Empty;
            this.Phone = user.aspnet_Users_Profile.FirstOrDefault() != null ? user.aspnet_Users_Profile.FirstOrDefault().Phone : string.Empty;
            this.Title = user.aspnet_Users_Profile.FirstOrDefault() != null ? user.aspnet_Users_Profile.FirstOrDefault().Title : string.Empty;
            this.UserRoles = System.Web.Security.Roles.GetRolesForUser(user.UserName);
            var employeeAspnetUserRel = user.Employee_aspnet_User_Rel.FirstOrDefault();
            if (employeeAspnetUserRel != null)
                this.EmployeeID = employeeAspnetUserRel.EmployeeID;
            EmployeeEdit = false;
            this.SelectedFranchises = this.FranchiseIds != null ? this.GetFranchises().Where(fre => this.FranchiseIds.Contains(fre.FranchiseID)).ToList() : new List<Franchise>();
        }
    }
}