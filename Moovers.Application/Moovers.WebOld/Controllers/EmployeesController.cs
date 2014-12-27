// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EmployeesController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    using Business.Enums;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using Moovers.WebModels;

    using MooversCRM.Controllers.BaseControllers;

    using File = Business.Models.File;

    public class EmployeesController : SecureBaseController
    {
        public ActionResult Index(
            EmployeeSort sort = EmployeeSort.Number,
            bool desc = false,
            bool inactive = false,
            EmployeeStatus status = EmployeeStatus.Active,
            int storenumber = 0)
        {
            List<Guid> franchises = AspUser.GetAllFranchises().Select(fra => fra.FranchiseID).ToList();

            var repo = new EmployeeRepository();
            IOrderedQueryable<Employee> employees = repo.GetAllIncludedArchived(franchises, status, storenumber);
            ViewBag.Inactive = inactive;
            return View(new EmployeeListModel(employees, sort, desc));
        }

        public ActionResult IndexNew(
            EmployeeSort sort = EmployeeSort.EmployeeID,
            bool desc = false,
            bool inactive = false,
            EmployeeStatus status = EmployeeStatus.Active,
            int storenumber = 0)
        {
            List<Guid> franchises = AspUser.GetAllFranchises().Select(fra => fra.FranchiseID).ToList();

            var repo = new EmployeeRepository();
            IOrderedQueryable<Employee> employees = repo.GetAllIncludedArchived(franchises, status, storenumber);
            ViewBag.Inactive = inactive;
            return View("IndexNew", new EmployeeListModel(employees, sort, desc, false));
        }

        public new ActionResult View(string id)
        {
            var repo = new EmployeeRepository();
            var model = new EditEmployeeModel(repo.Get(id), null);
            return View(model);
        }

        public ActionResult PayrollSummary(
            DateTime? start = null,
            DateTime? end = null,
            DateTime? now = null,
            PayrollSummarySort sort = PayrollSummarySort.Employee,
            bool desc = false,
            bool iscsv = false)
        {
            // end of the first payroll period
            if (!start.HasValue || !end.HasValue)
            {
                DateTime thisnow = now ?? DateTime.Now;
                start = Date.GetPayPeriodStart(thisnow);
                end = Date.GetPayPeriodEnd(thisnow);

                double daysBetween = Math.Floor((thisnow - start.Value).TotalDays);
                if (daysBetween >= 0 && daysBetween <= 5)
                {
                    DateTime? tmpstart = start;
                    start = Date.GetPayPeriodStart(tmpstart.Value.AddDays(-1));
                    end = Date.GetPayPeriodEnd(tmpstart.Value.AddDays(-1));
                }
            }

            var repo = new EmployeeRepository();
            IEnumerable<KeyValuePair<Employee, IQueryable<Posting_Employee_Rel>>> withPostings =
                repo.GetWithPostings(SessionFranchiseID, start.Value, end.Value).ToList().AsEnumerable();

            var quoteRepo = new QuoteRepository();
            IQueryable<Quote> valuationQuotes = quoteRepo.GetWithValuation(SessionFranchiseID, start.Value, end.Value);

            if (sort == PayrollSummarySort.Comm)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Commission) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.Hours)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Hours) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.ManHourRate)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Key.GetManHourRateBetween(start.Value, end.Value), !desc);
            }
            else if (sort == PayrollSummarySort.Moves)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Count(), !desc);
            }
            else if (sort == PayrollSummarySort.Tip)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Tip) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.Valuation)
            {
                withPostings = (from i in withPostings
                                let userid = (i.Key.Employee_aspnet_User_Rel.FirstOrDefault() ?? new Employee_aspnet_User_Rel()).aspnet_UserID
                                let quotes = valuationQuotes.Where(q => q.AccountManagerID == userid)
                                select new { withpost = i, userid = userid, valuations = quotes }).OrderWithDirection(
                        i =>
                            !i.valuations.Any()
                                ? 0
                                : i.valuations.Sum(v => v.ReplacementValuationCost ?? (v.ValuationTypeID.HasValue ? v.ReplacementValuation.Cost : 0)),
                        desc).Select(i => i.withpost);
            }
            else
            {
                withPostings = withPostings.OrderWithDirection(i => i.Key.NameLast + " " + i.Key.NameFirst, desc);
            }

            // MOOCRM-44 - filter to employees who are either still working here or have hours on this payroll
            withPostings = withPostings.Where(i => !i.Key.IsArchived || i.Value.Any() || i.Key.Employee_aspnet_User_Rel.Any());

            var model = new PayrollSummaryModel()
            {
                Employees = withPostings,
                StartDate = start.Value,
                EndDate = end.Value,
                Sort = sort,
                Desc = desc,
                ValuationQuotes = valuationQuotes
            };

            if (iscsv)
            {
                byte[] memoryArray = Encoding.UTF8.GetBytes(model.ToCsv());
                return File(memoryArray, "text/csv", String.Format("Payroll {0} to {1}.csv", start.Value.ToShortDateString(), end.Value.ToShortDateString()));
            }

            return View(model);
        }

        public ActionResult PayrollSummaryReport(
            string[] selectedNames,
            DateTime? start = null,
            DateTime? end = null,
            DateTime? now = null,
            PayrollSummarySort sort = PayrollSummarySort.Employee,
            bool desc = false,
            bool iscsv = false)
        {
            // end of the first payroll period
            if (!start.HasValue || !end.HasValue)
            {
                DateTime thisnow = now ?? DateTime.Now;
                start = Date.GetPayPeriodStart(thisnow);
                end = Date.GetPayPeriodEnd(thisnow);

                double daysBetween = Math.Floor((thisnow - start.Value).TotalDays);
                if (daysBetween >= 0 && daysBetween <= 5)
                {
                    DateTime? tmpstart = start;
                    start = Date.GetPayPeriodStart(tmpstart.Value.AddDays(-1));
                    end = Date.GetPayPeriodEnd(tmpstart.Value.AddDays(-1));
                }
            }

            var repo = new EmployeeRepository();
            IEnumerable<KeyValuePair<Employee, IQueryable<Posting_Employee_Rel>>> withPostings =
                repo.GetWithPostings(SessionFranchiseID, start.Value, end.Value).ToList().AsEnumerable();

            var quoteRepo = new QuoteRepository();
            IQueryable<Quote> valuationQuotes = quoteRepo.GetWithValuation(SessionFranchiseID, start.Value, end.Value);

            if (sort == PayrollSummarySort.Comm)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Commission) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.Hours)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Hours) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.ManHourRate)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Key.GetManHourRateBetween(start.Value, end.Value), !desc);
            }
            else if (sort == PayrollSummarySort.Moves)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Count(), !desc);
            }
            else if (sort == PayrollSummarySort.Tip)
            {
                withPostings = withPostings.OrderWithDirection(i => i.Value.Any() ? i.Value.Sum(p => p.Tip) : 0, !desc);
            }
            else if (sort == PayrollSummarySort.Valuation)
            {
                withPostings = (from i in withPostings
                                let userid = (i.Key.Employee_aspnet_User_Rel.FirstOrDefault() ?? new Employee_aspnet_User_Rel()).aspnet_UserID
                                let quotes = valuationQuotes.Where(q => q.AccountManagerID == userid)
                                select new { withpost = i, userid = userid, valuations = quotes }).OrderWithDirection(
                        i =>
                            !i.valuations.Any()
                                ? 0
                                : i.valuations.Sum(v => v.ReplacementValuationCost ?? (v.ValuationTypeID.HasValue ? v.ReplacementValuation.Cost : 0)),
                        desc).Select(i => i.withpost);
            }
            else
            {
                withPostings = withPostings.OrderWithDirection(i => i.Key.NameLast + " " + i.Key.NameFirst, desc);
            }

            // MOOCRM-44 - filter to employees who are either still working here or have hours on this payroll
            withPostings = withPostings.Where(i => !i.Key.IsArchived || i.Value.Any() || i.Key.Employee_aspnet_User_Rel.Any());

            var data = new PayrollSummaryModel()
            {
                Employees = withPostings,
                StartDate = start.Value,
                EndDate = end.Value,
                Sort = sort,
                Desc = desc,
                ValuationQuotes = valuationQuotes
            };

            if (iscsv)
            {
                byte[] memoryArray = Encoding.UTF8.GetBytes(data.ToCsv());
                return File(memoryArray, "text/csv", String.Format("Payroll {0} to {1}.csv", start.Value.ToShortDateString(), end.Value.ToShortDateString()));
            }

            return View(data);
        }

        public ActionResult GetJobList(string id, DateTime start, DateTime end)
        {
            var repo = new EmployeeRepository();
            Employee emp = repo.Get(id);

            IEnumerable<Posting_Employee_Rel> postings = emp.GetJobs(start, end);

            var quoteRepo = new QuoteRepository();

            Guid userid = emp.GetAspUserID();
            IQueryable<Quote> valuations = quoteRepo.GetWithValuation(SessionFranchiseID, start, end).Where(i => i.AccountManagerID == userid);
            var model = new EmployeeSummaryModel() { Employee = emp, StartDate = start, EndDate = end, Postings = postings, Valuations = valuations };

            return View(model);
        }

        public ActionResult GetEmployeesJobList(string[] selectedNames, DateTime start, DateTime end)
        {
            var repo = new EmployeeRepository();
            var summaryModels = new List<EmployeeSummaryModel>();
            var pdfs = new List<string>();
            if (selectedNames != null)
            {
                foreach (string id in selectedNames)
                {
                    Employee emp = repo.Get(id);

                    IEnumerable<Posting_Employee_Rel> postings = emp.GetJobs(start, end);

                    var quoteRepo = new QuoteRepository();

                    Guid userid = emp.GetAspUserID();
                    IQueryable<Quote> valuations = quoteRepo.GetWithValuation(SessionFranchiseID, start, end).Where(i => i.AccountManagerID == userid);
                    summaryModels.Add(
                        new EmployeeSummaryModel() { Employee = emp, StartDate = start, EndDate = end, Postings = postings, Valuations = valuations });
                    var empModel = new EmployeeSummaryModel() { Employee = emp, StartDate = start, EndDate = end, Postings = postings, Valuations = valuations };
                    string html = RenderViewToString("GetEmployeesJobList", empModel);
                    var file = new File("JOB - LIST " + emp.Lookup, "application/pdf");
                    pdfs.Add(html);
                }
            }
            byte[] pdf = General.GeneratePdf("<!DOCTYPE html><html>" + String.Join(String.Empty, pdfs) + "</html>", PaperKind.Legal);
            return File(pdf, "application/pdf");
        }

        public ActionResult ViewFile(Guid id)
        {
            var repo = new FileRepository();
            File file = repo.Get(id);

            return File(file.SavedContent, file.ContentType);
        }

        public ActionResult Edit(string id)
        {
            var repo = new EmployeeRepository();
            var franchiseRelRepo = new Franchise_aspnet_UserRepository();
            Employee emp = repo.Get(id);
            aspnet_User usr = null;
            string title = "";
            if (emp != null)
            {
                if (emp.Employee_aspnet_User_Rel.Any())
                {
                    usr = emp.Employee_aspnet_User_Rel.First().aspnet_Users;

                    var profileRepo = new aspnet_Users_ProfileRepository();
                    MembershipUser memuser = Membership.GetUser(usr.UserName);
                    aspnet_Users_Profile profile = profileRepo.Get(usr.UserId);
                    title = profile.Title;
                    emp.Email = new EmailAddress
                    {
                        Email = memuser.Email
                    };


                }
            }



            string username = usr != null ? usr.UserName : string.Empty;
            var allRoles = Roles.GetAllRoles();
            Guid franchiseid = emp != null
                ? emp.Employee_aspnet_User_Rel.Any()
                    ? franchiseRelRepo.GetByUserId(emp.Employee_aspnet_User_Rel.First().aspnet_Users.UserId).First().FranchiseID
                    : SessionFranchiseID
                : SessionFranchiseID;
            bool islocked = emp != null
                            && (emp.Employee_aspnet_User_Rel.Any() && emp.Employee_aspnet_User_Rel.First().aspnet_Users.aspnet_Membership.IsLockedOut);
            var model = new EditEmployeeModel(emp, username, !string.IsNullOrEmpty(username), franchiseid, usr) { IsLocked = islocked };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, EditEmployeeModel model, HttpPostedFileBase driverlicenseimg
            , HttpPostedFileBase dotcardimg, string workingtype, string classtype, Guid? franchiseid = null,
            Guid? locationid = null)
        {
            if (workingtype.Equals(WorkingType.Needed.ToString()))
            {

                workingtype = ((int)WorkingType.Needed).ToString();
            }
            else if(workingtype.Equals(WorkingType.Regular.ToString()))
            {
                workingtype = ((int)WorkingType.Regular).ToString();

            }

            if (classtype.Equals(ClassType.FullTime.ToString()))
            {
                classtype = ((int)ClassType.FullTime).ToString();


            }
            else if(classtype.Equals(ClassType.PartTime.ToString()))
            {
                classtype = ((int)ClassType.PartTime).ToString();
            }

            
            var repo = new EmployeeRepository();
            Employee emp = repo.Get(id);
            bool UserDone = false;
            if (emp != null)
            {
                //already = false;

                if (!AspUser.HasPermissionsOn(emp.FranchiseID))
                {
                    return HttpNotFound();
                }

                if (model.Username != null && (!string.IsNullOrEmpty(model.Password) || !string.IsNullOrEmpty(model.ConfirmPassword)))
                {
                    UserDone = false;

                    if (model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("Password", "Please ensure your passwords match");
                        return View(model);
                    }
                    else if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                    {
                        ModelState.AddModelError("ConfirmPassword", "Please fill both Password and Confirm Password fields");
                        return View(model);
                    }
                    else
                    {
                        MembershipUser aspuser = Membership.GetUser(model.Username);

                        if (aspuser != null)
                        {
                            try
                            {
                                string pass = aspuser.ResetPassword();
                                aspuser.ChangePassword(pass, model.Password);
                                aspuser.UnlockUser();
                                Membership.UpdateUser(aspuser);
                                UserDone = true;

                                //if (!Roles.GetRolesForUser(aspuser.UserName).Contains("FieldEmployee"))
                                //{
                                //    Roles.AddUserToRole(aspuser.UserName, "FieldEmployee");
                                //}

                            }
                            catch (Exception)
                            {
                                ModelState.AddModelError(
                                    "Password",
                                    "Please ensure your password has a letter, a number, a symbol and minimum of length of six.");
                                return View(model);
                            }
                        }
                        else
                        {
                            MembershipCreateStatus status;
                            Membership.CreateUser(model.Username, model.Password, model.email, null, null, true, out status);

                            if (status == MembershipCreateStatus.DuplicateEmail)
                            {
                                ModelState.AddModelError("email", "E-Mail address is already in use");
                            }
                            else if (status == MembershipCreateStatus.DuplicateUserName)
                            {
                                ModelState.AddModelError("Username", "Username is already in use");
                            }
                            else if (status == MembershipCreateStatus.InvalidEmail)
                            {
                                ModelState.AddModelError("email", "Invalid e-mail address");
                            }
                            else if (status == MembershipCreateStatus.InvalidPassword)
                            {
                                ModelState.AddModelError(
                                    "Password",
                                    "Please ensure your password has at least 1 uppercase letter, 1 lowercase letter, and 1 non-letter character.");
                            }
                            else if (status == MembershipCreateStatus.InvalidUserName)
                            {
                                ModelState.AddModelError("Username", "Invalid Username");
                            }

                            if (status != MembershipCreateStatus.Success)
                            {
                                return View(model);
                            }
                            UserDone = true;
                            //already = false;
                        }
                    }
                }
            }
            else
            {
                emp = new Employee
                {
                    // Users with multiple franchises are shown a dropdown to select the franchise, otherwise, use the default franchise
                    FranchiseID = (AspUser.HasMultipleFranchises()) ? franchiseid.Value : SessionFranchiseID
                };
                repo.Add(emp);
                //repo.Save();
            }





                model.UpdateEmployee(ref emp);
                emp.FranchiseID = franchiseid.HasValue ? franchiseid.Value : emp.FranchiseID;
                emp.LocationId = locationid.HasValue ? locationid.Value : emp.LocationId;
                emp.TypeId = int.Parse(workingtype);
                emp.ClassId = int.Parse(classtype);
                repo.Save();
            






            if (model.Username != null && (!string.IsNullOrEmpty(model.Password) || !string.IsNullOrEmpty(model.ConfirmPassword)) && !UserDone)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("Password", "Please ensure your passwords match");
                    return View(model);
                }
                else if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.AddModelError("Password", "Please fill both Password and Confirm Password fields");
                    return View(model);
                }
                else
                {
                    MembershipCreateStatus status;
                    Membership.CreateUser(model.Username, model.Password, model.email, null, null, true, out status);

                    if (status == MembershipCreateStatus.DuplicateEmail)
                    {
                        ModelState.AddModelError("email", "E-Mail address is already in use");
                    }
                    else if (status == MembershipCreateStatus.DuplicateUserName)
                    {
                        ModelState.AddModelError("Username", "Username is already in use");
                    }
                    else if (status == MembershipCreateStatus.InvalidEmail)
                    {
                        ModelState.AddModelError("email", "Invalid e-mail address");
                    }
                    else if (status == MembershipCreateStatus.InvalidPassword)
                    {
                        ModelState.AddModelError(
                            "Password",
                            "Please ensure your password has at least 1 uppercase letter, 1 lowercase letter, and 1 non-letter character.");
                    }
                    else if (status == MembershipCreateStatus.InvalidUserName)
                    {
                        ModelState.AddModelError("Username", "Invalid Username");
                    }

                    if (status != MembershipCreateStatus.Success)
                    {
                        return View(model);
                    }



                    model.EmployeeId = emp.EmployeeID;
                    //update title
                }
            }
            var ty = model.UserRoles;
            if (!string.IsNullOrEmpty(model.Username))
            {
                MembershipUser usr = Membership.GetUser(model.Username);
                if (usr != null)
                {

                    var strRoles = Roles.GetRolesForUser(model.Username);
                    if (strRoles.Count() > 0)
                    {
                        Roles.RemoveUserFromRoles(model.Username, strRoles);
                    }

                    Roles.AddUserToRole(model.Username, model.Roles.ElementAt(0));

                }
            }







            if (driverlicenseimg != null && driverlicenseimg.ContentLength > 0)
            {
                using (Stream inputStream = driverlicenseimg.InputStream)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        inputStream.CopyTo(memoryStream);
                        byte[] data = memoryStream.ToArray();
                        emp.AddFile(Employee_File_Type.DriverLicense, "driverlicense-" + emp.EmployeeID.ToString(), data, driverlicenseimg.ContentType);
                    }
                }
            }

            if (dotcardimg != null && dotcardimg.ContentLength > 0)
            {
                using (Stream inputstream = dotcardimg.InputStream)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        inputstream.CopyTo(memoryStream);
                        byte[] data = memoryStream.ToArray();
                        emp.AddFile(Employee_File_Type.DOTCard, "dotcard-" + emp.EmployeeID.ToString(), data, dotcardimg.ContentType);
                    }
                }
            }

            SetFranchise(franchiseid, model.Username);
            //SetFieldEmployeeRole(model.Username);
            model.EmployeeId = emp.EmployeeID;
            CreateAspNetUserProfile(model);
            LockUnlockUser(model.Username, model.IsLocked);
            repo.Save();

            // lookups are generated by a trigger, which isn't reflected unless we fetch from a new repo
            string lookup = new EmployeeRepository().Get(emp.EmployeeID).Lookup;
            return RedirectToAction("View", new { id = lookup });
        }

        public void LockUnlockUser(string username, bool islocked)
        {
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            var userrepo = new aspnet_UserRepository();
            var usermembershiprepo = new AspnetUsersMembershipRepository();

            aspnet_User user = userrepo.Get(username);
            aspnet_Membership usermembership = usermembershiprepo.Get(user.UserId);

            usermembership.IsLockedOut = islocked;
            usermembershiprepo.Save();
        }

        [HttpPost]
        public ActionResult Remove(string id, DateTime? terminationDate, string reason, TerminationReasons terminationType)
        {
            var repo = new EmployeeRepository();
            Employee emp = repo.Get(id);

            if (emp == null)
            {
                return HttpNotFound();
            }

            emp.TerminationDate = terminationDate;
            emp.TerminationReason = reason;
            emp.TerminationType = (int)terminationType;
            emp.IsArchived = true;
            repo.Save();
            return RedirectToAction("Index");
        }

        public ActionResult LastPayPeriod(DateTime currentStart)
        {
            DateTime newstart = Date.GetPayPeriodStart(currentStart.AddDays(-1));
            DateTime newend = Date.GetPayPeriodEnd(currentStart.AddDays(-1));

            return RedirectToAction("PayrollSummary", "Employees", new { start = newstart, end = newend });
        }

        public ActionResult NextPayPeriod(DateTime currentEnd)
        {
            DateTime newstart = Date.GetPayPeriodStart(currentEnd.AddDays(1));
            DateTime newend = Date.GetPayPeriodEnd(currentEnd.AddDays(1));
            return RedirectToAction("PayrollSummary", "Employees", new { start = newstart, end = newend });
        }

        private void SetFranchise(Guid? franchiseid, string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            var asprepo = new aspnet_UserRepository();
            aspnet_User user = asprepo.Get(username);

            var franchiseRelRepo = new Franchise_aspnet_UserRepository();

            if (franchiseid.HasValue)
            {
                IEnumerable<Franchise_aspnetUser> franchises = franchiseRelRepo.GetByUserId(user.UserId);

                if (franchises.Count() == 1 && franchises.First().FranchiseID == franchiseid.Value)
                {
                    return;
                }

                franchiseRelRepo.RemoveForUser(user.UserId);

                franchiseRelRepo.Add(new Franchise_aspnetUser { FranchiseID = franchiseid.Value, UserID = user.UserId });
            }
            else
            {
                franchiseRelRepo.Add(new Franchise_aspnetUser { FranchiseID = SessionFranchiseID, UserID = user.UserId });
            }

            franchiseRelRepo.Save();
        }

        private void CreateAspNetUserProfile(EditEmployeeModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                return;
            }

            var asprepo = new aspnet_UserRepository();
            var profilerepo = new aspnet_Users_ProfileRepository();
            var emoplyrelationrepo = new Employee_aspnet_User_RelRepository();

            aspnet_User user = asprepo.Get(model.Username);

            if (profilerepo.Get(user.UserId) == null)
            {
                var profile = new aspnet_Users_Profile()
                {
                    FirstName = model.firstname,
                    LastName = model.lastname,
                    Phone = model.primaryPhone,
                    Title = "",
                    PrivateKey = Guid.NewGuid(),
                    UserID = user.UserId
                };
                profilerepo.Add(profile);
                profilerepo.Save();
            }

            if (emoplyrelationrepo.Get(user.UserId) == null)
            {
                var employeerelation = new Employee_aspnet_User_Rel() { EmployeeID = model.EmployeeId, aspnet_UserID = user.UserId };

                emoplyrelationrepo.Add(employeerelation);
                emoplyrelationrepo.Save();
            }
        }

        private void SetFieldEmployeeRole(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            if (!Roles.GetRolesForUser(username).Contains("FieldEmployee"))
            {
                Roles.AddUserToRole(username, "FieldEmployee");
            }
        }
    }
}