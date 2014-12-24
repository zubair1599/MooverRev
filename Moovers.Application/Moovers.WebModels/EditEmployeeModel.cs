// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EditEmployeeModel.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebModels
{
    using System;
    using System.Collections.Generic;

    using Business.Enums;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using System.Web.Mvc;
    using System.Reflection;
    using System.Linq;

    public class EditEmployeeModel
    {
        public EditEmployeeModel()
        {
            //this.UserRoles = System.Web.Security.Roles.GetRolesForUser();
        }

        public EditEmployeeModel(Employee employee , aspnet_User usr)
        {

            
            employee = employee ?? new Employee();

            Employee = employee;
            this.Title = employee.Title;

            EmployeeId = employee.EmployeeID;

            lookup = employee.Lookup;
            firstname = employee.NameFirst;
            lastname = employee.NameLast;
            ssn = employee.SocialSecurityNumber;
            driverlicense = employee.DriverLicense;
            gender = employee.Gender;
            birthdate = employee.BirthDate;
            EmployeeNumber = employee.EmployeeNumber;
            primaryPhone = (employee.PrimaryPhone != null) ? employee.PrimaryPhone.DisplayString() : null;
            secondaryPhone = (employee.SecondaryPhone != null) ? employee.SecondaryPhone.DisplayString() : null;
            acceptTextPrimary = employee.PrimaryPhoneAcceptText;
            acceptTextSecondary = employee.SecondaryPhoneAcceptText;
            email = (employee.Email != null) ? employee.Email.Email : null;
            isArchived = employee.IsArchived;
            if (employee.Address != null)
            {
                street1 = employee.Address.Street1;
                street2 = employee.Address.Street2;
                city = employee.Address.City;
                state = employee.Address.State;
                zip = employee.Address.Zip;
            }

            position = employee.PositionType;
            payType = employee.PayType;
            payRate = employee.Wage;
            filingStatus = employee.FilingStatus;
            allowance = employee.Allowance;
            driverLicenseExpirationMonth = employee.DriverLicenseExpirationMonth;
            driverLicenseExpirationYear = employee.DriverLicenseExpirationYear;

            terminationDate = employee.TerminationDate;
            terminationType = (TerminationReasons?)employee.TerminationType;
            terminationReason = employee.TerminationReason;
            hireDate = employee.HireDate;

            FranchiseId = employee.FranchiseID;
            LocationId = employee.LocationId;


            this.User = usr;
            Guid userId =  employee.GetAspUserID();
            aspnet_User currentUserforEmp = null;
            foreach (var item in employee.Employee_aspnet_User_Rel)
            {
                if (item.aspnet_UserID.Equals(userId))
                {
                    currentUserforEmp = item.aspnet_Users;
                    break;
                }
            }
            if (currentUserforEmp!=null)
            {
                
                this.UserRoles = System.Web.Security.Roles.GetRolesForUser(currentUserforEmp.UserName);
                this.User = currentUserforEmp;
            }
            this.Roles = System.Web.Security.Roles.GetAllRoles();

        }

        public EditEmployeeModel(Employee employee, string username, bool isEdit, Guid franchiseId,aspnet_User usr) : this(employee,usr)
        {
            
            Username = username;
            IsEdit = isEdit;
            FranchiseId = franchiseId;
            this.User = usr;
            if (employee!=null)
            {
                if (employee.Email!=null)
                {
                    this.email = employee.Email.Email;    
                }
                
                    this.EmployeeNumber = employee.EmployeeNumber;
                    this.Title = employee.Title;
            }
            
            
            
        }
        



        public int ClassID { get; set; }
        public int TypeID { get; set; }

        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
        public Business.Models.aspnet_User User { get; set; }

        public string Title { get; set; }
        public int EmployeeNumber { get; set; }



        public Guid FranchiseId { get; set; }

        public Guid? LocationId { get; set; }

        public bool IsEdit { get; set; }

        public Employee Employee { get; set; }

        public Guid EmployeeId { get; set; }

        public string lookup { get; set; }

        public string firstname { get; set; }

        public string lastname { get; set; }

        public string ssn { get; set; }

        public string driverlicense { get; set; }

        public Gender gender { get; set; }

        public DateTime? birthdate { get; set; }

        public string primaryPhone { get; set; }

        public string secondaryPhone { get; set; }

        public bool acceptTextPrimary { get; set; }

        public bool acceptTextSecondary { get; set; }

        public bool isArchived { get; set; }

        public string email { get; set; }

        public string street1 { get; set; }

        public string street2 { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip { get; set; }

        public Position position { get; set; }

        public PayType payType { get; set; }

        public FilingStatus filingStatus { get; set; }

        public decimal? payRate { get; set; }

        public int? allowance { get; set; }

        public int? driverLicenseExpirationMonth { get; set; }

        public int? driverLicenseExpirationYear { get; set; }

        public DateTime? terminationDate { get; set; }

        public TerminationReasons? terminationType { get; set; }

        public string terminationReason { get; set; }

        public DateTime? hireDate { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool IsLocked { get; set; }

        public IEnumerable<Franchise> GetFranchises()
        {
            var repo = new FranchiseRepository();
            return repo.GetAll();
        }

        public IEnumerable<Location> GetLocations()
        {
            var repo = new LocationRepository();
            return repo.GetAll();
        }

        public void UpdateEmployee(ref Employee emp)
        {
            emp.Title = Title;
            emp.EmployeeNumber = EmployeeNumber;

            emp.NameFirst = firstname;
            emp.NameLast = lastname;
            emp.SocialSecurityNumber = ssn;
            emp.DriverLicense = driverlicense;
            emp.Gender = gender;
            emp.BirthDate = birthdate;
            emp.PrimaryPhone = new PhoneNumber(primaryPhone);
            emp.SecondaryPhone = new PhoneNumber(secondaryPhone);
            emp.PrimaryPhoneAcceptText = acceptTextPrimary;
            emp.SecondaryPhoneAcceptText = acceptTextSecondary;
            emp.IsArchived = isArchived;
            emp.PositionType = position;

            if (!String.IsNullOrEmpty(email))
            {
                emp.Email = new EmailAddress(email);
            }
            else
            {
                emp.Email = null;
            }

            if (!String.IsNullOrEmpty(street1))
            {
                if (emp.Address == null)
                {
                    emp.Address = new Address();
                }

                emp.Address.Street1 = street1;
                emp.Address.Street2 = street2;
                emp.Address.City = city;
                emp.Address.State = state;
                emp.Address.Zip = zip;
            }
            else
            {
                emp.Address = null;
            }

            emp.PayType = payType;
            emp.Wage = payRate;
            emp.FilingStatus = filingStatus;
            emp.Allowance = allowance;
            emp.DriverLicenseExpirationMonth = driverLicenseExpirationMonth;
            emp.DriverLicenseExpirationYear = driverLicenseExpirationYear;

            emp.HireDate = hireDate;
            emp.TerminationDate = terminationDate;
            emp.TerminationType = (int?)terminationType;
            emp.TerminationReason = terminationReason;
        }
    }
}