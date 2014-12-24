using Business.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooversCRM.Views.Admin.ViewModels
{
    public class EmployeeLoginViewModel
    {
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Employee")]
        public Guid EmployeeID { get; set; }

        public IEnumerable<Employee> Employees { get; set; }
    }
}