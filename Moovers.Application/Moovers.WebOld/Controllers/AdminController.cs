// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="AdminController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using System.Web.Security;

    using Business.Enums;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using FluentValidation.Results;

    using Moovers.WebModels;
    using Moovers.WebModels.Validators;

    using MooversCRM.Controllers.BaseControllers;
    using MooversCRM.Views.Admin.ViewModels;

    using Newtonsoft.Json;

    //[Authorize(Roles = "Administrator")]
    public class AdminController : SecureBaseController
    {
        public EmployeeRepository EmployeeeRepository = new EmployeeRepository();

        public EmployeeAuthenticationRepository EmployeeAuthRepository;

        private readonly ICacheRepository _cacheRepository;

        public AdminController()
        {
            _cacheRepository = new CacheRepository();
            EmployeeAuthRepository = new EmployeeAuthenticationRepository(_cacheRepository);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ClearCache()
        {
            _cacheRepository.Clear();
            return RedirectToAction("Index");
        }

        public ActionResult CreateAccount()
        {
            return View(new AccountEditModel());
        }

        public ActionResult SetMaxTrucks()
        {
            var grepo = new GlobalSettingRepository(SessionFranchiseID);
            var maxTrucks = grepo.GetValue<int>(SettingTypes.MaxTrucks);
            ViewData["maxTrucks"] = maxTrucks;

            var repo = new FranchiseRepository();
            return View(repo.GetAll());
        }

        [HttpPost]
        public ActionResult SetMaxTrucks(Guid franchiseid, int maxtrucks)
        {
            var repo = new GlobalSettingRepository(franchiseid);
            repo.SetValue(SettingTypes.MaxTrucks, maxtrucks.ToString());
            repo.Save();
            return RedirectToAction("SetMaxTrucks");
        }

        public ActionResult SetTrucks()
        {
            var repo = new CustomCrewCountRepository();
            ObjectSet<CustomCrewCount> crews = repo.db.CustomCrewCounts;
            return View(crews);
        }

        public ActionResult RemoveCrewCount(Guid id)
        {
            var repo = new CustomCrewCountRepository();
            repo.Remove(id);
            repo.Save();
            return RedirectToAction("SetTrucks");
        }

        [HttpPost]
        public ActionResult SetTrucks(Guid franchiseid, int trucks, DateTime day)
        {
            var repo = new CustomCrewCountRepository();
            if (!repo.GetForDay(day, franchiseid).HasValue)
            {
                var crew = new CustomCrewCount() { FranchiseID = franchiseid, Count = trucks, Day = day.Day, Month = day.Month, Year = day.Year };

                repo.Add(crew);
                repo.Save();
            }
            return RedirectToAction("SetTrucks");
        }

        [HttpGet]
        public ActionResult SetHolidays()
        {
            var repo = new ScheduleNoteRepository();
            return View(repo.GetAll());
        }

        [HttpPost]
        public ActionResult SetHolidays(string message, DateTime day)
        {
            var repo = new ScheduleNoteRepository();
            var note = new ScheduleNote() { Day = day.Day, Month = day.Month, Year = day.Year, Message = message };
            repo.Add(note);
            repo.Save();
            return RedirectToAction("SetHolidays");
        }

        public ActionResult RemoveHoliday(Guid id)
        {
            var repo = new ScheduleNoteRepository();
            repo.Remove(id);
            repo.Save();
            return RedirectToAction("SetHolidays");
        }

        public ActionResult ViewStorage(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote == null)
            {
                return HttpNotFound();
            }

            return View(quote);
        }

        public ActionResult UpdateInventoryFromMP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateInventoryFromMP(string id, string invdata)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            quote.AddInventoryFromExternal(invdata);
            repo.Save();
            return RedirectToAction("UpdateInventoryFromMP");
        }

        public ActionResult Encrypt(string id)
        {
            return Content(Security.Encrypt(id));
        }

        public ActionResult Decrypt(string id)
        {
            return Content(Security.Decrypt(id));
        }

        [HttpPost]
        public ActionResult CreateAccount(IEnumerable<string> roles, AccountEditModel model)
        {
            roles = (roles ?? Enumerable.Empty<string>()).ToList();
            var validator = new UserAccountValidator();
            ValidationResult results = validator.Validate(model);

            foreach (ValidationFailure r in results.Errors)
            {
                ModelState.AddModelError(r.PropertyName, r.ErrorMessage);
            }

            if (results.IsValid)
            {
                MembershipCreateStatus status;
                Membership.CreateUser(model.User.UserName, model.Password, model.Email, null, null, true, out status);

                if (status == MembershipCreateStatus.DuplicateEmail)
                {
                    ModelState.AddModelError("Email", "E-Mail address is already in use");
                }
                else if (status == MembershipCreateStatus.DuplicateUserName)
                {
                    ModelState.AddModelError("Username", "Username is already in use");
                }
                else if (status == MembershipCreateStatus.InvalidEmail)
                {
                    ModelState.AddModelError("Email", "Invalid e-mail address");
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

                foreach (string r in roles)
                {
                    if (!Roles.RoleExists(r))
                    {
                        Roles.CreateRole(r);
                    }
                }

                if (roles.Any())
                {
                    Roles.AddUserToRoles(model.User.UserName, roles.ToArray());
                }

                string body = String.Format(@"Your credentials for <a href=""http://crm.1800moovers.com"">http://crm.1800moovers.com</a> is 
                    <table>
                        <tr>
                            <th>Username:</th>
                            <td>{0}</td>
                        </tr>
                        <tr>
                            <th>Password:</th>
                            <td>{1}</td>
                        </tr>
                    </table>", model.User.UserName, Regex.Replace(model.Password, ".", "*"));

                string subject = "Your Moovers CRM Credentials";
                Email.SendEmail(model.Email, "support@1800moovers.com", subject, body);
                var repo = new aspnet_UserRepository();
                aspnet_User user = repo.Get(model.User.UserName);

                var franchiseRelRepo = new Franchise_aspnet_UserRepository();

                if (model.FranchiseIds != null && model.FranchiseIds.Any())
                {
                    foreach (Guid franchiseId in model.FranchiseIds)
                    {
                        franchiseRelRepo.Add(new Franchise_aspnetUser { FranchiseID = franchiseId, UserID = user.UserId });
                    }
                }
                else
                {
                    franchiseRelRepo.Add(new Franchise_aspnetUser { FranchiseID = SessionFranchiseID, UserID = user.UserId });
                }

                franchiseRelRepo.Save();

                var profile = new aspnet_Users_Profile()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Title = model.Title,
                    PrivateKey = Guid.NewGuid()
                };

                user.aspnet_Users_Profile.Add(profile);

                user.Employee_aspnet_User_Rel.Add(new Employee_aspnet_User_Rel() { EmployeeID = model.EmployeeID });

                repo.Save();
                return RedirectToAction("EditAccounts", user.UserId);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult EditAccounts(Guid? id)
        {
            var repo = new aspnet_UserRepository();
            if (!id.HasValue)
            {
                return View(repo.GetAll());
            }

            aspnet_User employee = repo.GetNonCachedUser(id.Value);
            return View(employee);
        }

        [HttpPost]
        public ActionResult EditAccounts(Guid id, AccountEditModel model)
        {
            var repo = new aspnet_UserRepository();

            var profileRepo = new aspnet_Users_ProfileRepository();

            aspnet_User user = repo.Get(id);

            MembershipUser memuser = Membership.GetUser(user.UserName);

            aspnet_Users_Profile profile = profileRepo.Get(user.UserId);

            if (model.ConfirmPassword != model.Password)
            {
                ModelState.AddModelError("Password", "Please ensure your passwords match");
            }

            if (!profile.PrivateKey.HasValue || model.FirstName != profile.FirstName || model.LastName != profile.LastName || model.Phone != profile.Phone
                || model.Title != profile.Title)
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.Title = model.Title;
                profile.Phone = model.Phone;

                if (!profile.PrivateKey.HasValue)
                {
                    profile.PrivateKey = Guid.NewGuid();
                }
                profileRepo.Save();
            }

            if (user.Employee_aspnet_User_Rel.Count == 0)
            {
                user.Employee_aspnet_User_Rel.Add(new Employee_aspnet_User_Rel() { EmployeeID = model.EmployeeID });
                repo.Save();
            }

            if (ModelState.IsValid)
            {
                if (model.Password != AccountEditModel.UnchangedPasswordText)
                {
                    try
                    {
                        string oldPass = memuser.ResetPassword();
                        memuser.ChangePassword(oldPass, model.Password);
                        memuser.UnlockUser();

                        Membership.UpdateUser(memuser);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("Password", "Please ensure your password has a letter, a number, and a symbol");
                    }
                }

                if (model.Email != memuser.Email)
                {
                    memuser.Email = model.Email;
                    Membership.UpdateUser(memuser);
                }
                    var alloldroles = Roles.GetRolesForUser(user.UserName);
                    if(alloldroles!=null && alloldroles.Count()>0)
                    {
                        Roles.RemoveUserFromRoles(user.UserName, alloldroles);
                    }
                foreach (string role in model.Roles)
                {
                   

                    if (!Roles.RoleExists(role))
                    {
                        Roles.CreateRole(role);
                    }

                    if (!Roles.IsUserInRole(user.UserName, role))
                    {
                        Roles.AddUserToRole(user.UserName, role);
                    }
                }

                var franchiserepo = new Franchise_aspnet_UserRepository();

                if (model.FranchiseIds != null && model.FranchiseIds.Any())
                {
                    franchiserepo.RemoveForUser(user.UserId);

                    foreach (Guid franchiseid in model.FranchiseIds)
                    {
                        franchiserepo.Add(new Franchise_aspnetUser() { FranchiseID = franchiseid, UserID = user.UserId });
                    }
                }
                else if (model.FranchiseIds == null)
                {
                    franchiserepo.RemoveForUser(user.UserId);
                    franchiserepo.Add(new Franchise_aspnetUser() { FranchiseID = SessionFranchiseID, UserID = user.UserId });
                }

                franchiserepo.Save();

                return RedirectToAction("EditAccounts", new { id = id });
            }

            return View(model);
        }

        public ActionResult DeleteUserAccount(string username)
        {
            MembershipUser user = Membership.GetUser(username);

            var franchiserepo = new Franchise_aspnet_UserRepository();
            franchiserepo.RemoveForUser((Guid)user.ProviderUserKey);
            franchiserepo.Save();

            var quotelogrepo = new QuoteStatusLogRepository();
            quotelogrepo.RemoveForUser((Guid)user.ProviderUserKey);
            quotelogrepo.Save();

            var userprofilerepo = new aspnet_Users_ProfileRepository();
            userprofilerepo.RemoveForUser((Guid)user.ProviderUserKey);
            userprofilerepo.Save();

            var quoteaccessrepo = new QuoteAccessLogRepository();
            quoteaccessrepo.RemoveForUser((Guid)user.ProviderUserKey);
            quoteaccessrepo.Save();

            var empuserrelrepo = new Employee_aspnet_User_RelRepository();
            empuserrelrepo.RemoveForUser((Guid)user.ProviderUserKey);
            empuserrelrepo.Save();

            var quotecommentrepo = new QuoteCommentRepository();
            quotecommentrepo.RemoveForUser((Guid)user.ProviderUserKey);
            quotecommentrepo.Save();

            var personaccountrelrepo = new PersonAccountRepository();
            personaccountrelrepo.RemoveForUser((Guid)user.ProviderUserKey);
            personaccountrelrepo.Save();

            Membership.DeleteUser(username, true);
            return RedirectToActionPermanent("EditAccounts");
        }

        public ActionResult TestError(string error)
        {
            throw new Exception(error);
        }

        public ActionResult Errors(int page = 0, int take = 100)
        {
            var repo = new ErrorRepository();
            return View(repo.GetPage(page, take));
        }

        public ActionResult ErrorView(Guid id)
        {
            var repo = new ErrorRepository();
            return View(repo.Get(id));
        }

        public ActionResult Inventory()
        {
            var repo = new InventoryItemRepository();
            return View(repo.GetUnarchived());
        }

        public ActionResult AddFranchise(bool success = false)
        {
            ViewBag.Success = success;
            return View();
        }

        [HttpPost]
        public ActionResult AddFranchise(AddressModel address, string name)
        {
            var repo = new FranchiseRepository();

            var franchise = new Franchise { Name = name, Address = address.GetAddress() };

            repo.Add(franchise);
            repo.Save();
            return RedirectToAction("AddFranchise", new { success = true });
        }

        [HttpPost]
        public ActionResult RemoveItem(Guid id)
        {
            var repo = new InventoryItemRepository();
            InventoryItem item = repo.Get(id);

            if (item == null)
            {
                return HttpNotFound();
            }

            repo.Remove(item);
            repo.Save();
            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public ActionResult Inventory(Guid? oldID, int keyCode, string name, string pluralName, string aliases, int cubicFeet, int weight, bool isBox)
        {
            var repo = new InventoryItemRepository();

            InventoryItem existingKeyCode = repo.GetByKeyCode(keyCode);
            if (existingKeyCode != null && (!oldID.HasValue || oldID.Value == existingKeyCode.ItemID))
            {
                ModelState.AddModelError("KeyCode", "Keycode already exists in the system, please use a unique keycode");
                return View(repo.GetUnarchived());
            }

            var oldItem = new InventoryItem();
            if (oldID.HasValue)
            {
                oldItem = repo.Get(oldID.Value);
            }

            IEnumerable<string> aliasList = aliases.Split(',').Where(a => !String.IsNullOrWhiteSpace(a)).Select(a => a.Trim());

            if (oldItem.Name == name && oldItem.PluralName == pluralName && oldItem.CubicFeet == cubicFeet && oldItem.Weight == weight && oldItem.IsBox == isBox)
            {
                oldItem.KeyCode = keyCode;
                oldItem.Aliases = aliasList;
                repo.Save();
            }
            else
            {
                InventoryItem item = oldItem.Duplicate();
                item.KeyCode = keyCode;
                item.Name = name;
                item.PluralName = pluralName;
                item.Aliases = aliasList;
                item.CubicFeet = cubicFeet;
                item.Weight = weight;
                item.IsBox = isBox;
                item.NewRevisionItemID = oldID;
                repo.Add(item);
                repo.Save();
            }

            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public ActionResult AddInventoryQuestions(Guid id)
        {
            var repo = new InventoryItemRepository();
            return View(repo.Get(id));
        }

        [HttpPost]
        public ActionResult AddQuestion(
            Guid itemid,
            string questionText,
            string shortName,
            decimal? additionalCubicFeet,
            decimal? additionalTime,
            decimal? additionalWeight,
            IEnumerable<string> options = null)
        {
            var repo = new InventoryItemRepository();
            InventoryItem item = repo.Get(itemid);
            int newSort = item.InventoryItemQuestions.Any() ? item.InventoryItemQuestions.Max(i => i.Sort) : 0;

            options = options ?? Enumerable.Empty<string>();
            var question = new InventoryItemQuestion
            {
                QuestionText = questionText,
                ShortName = shortName,
                CubicFeet = additionalCubicFeet,
                Time = additionalTime,
                Sort = newSort,
                Weight = additionalWeight
            };

            foreach (string optionjson in options)
            {
                var obj = General.DeserializeJson<dynamic>(optionjson);
                var option = new InventoryItemQuestionOption();

                decimal? time = !String.IsNullOrEmpty((string)obj.time) ? decimal.Parse((string)obj.time) : (decimal?)null;
                decimal? weight = !String.IsNullOrEmpty((string)obj.weight) ? decimal.Parse((string)obj.weight) : (decimal?)null;
                decimal? cubicFeet = !String.IsNullOrEmpty((string)obj.cubicFeet) ? decimal.Parse((string)obj.cubicFeet) : (decimal?)null;
                bool selected = !String.IsNullOrEmpty((string)obj.cubicFeet) && bool.Parse((string)obj.selected);

                option.CubicFeet = cubicFeet;
                option.Selected = selected;
                option.Time = time;
                option.Weight = weight;
                option.Option = obj.option;
                option.Sort = obj.sort;
                question.InventoryItemQuestionOptions.Add(option);
            }

            item.InventoryItemQuestions.Add(question);
            repo.Save();

            return RedirectToAction("AddInventoryQuestions", new { id = itemid });
        }

        public ActionResult DisplayMoovers()
        {
            return View();
        }

        public ActionResult TodayMoves()
        {
            IQueryable<QuoteMapDirection> list = new QuoteRepository().GetMooversDirections(DateTime.Today);
            List<QuoteDirectionViewModel> dirList = null;
            var moves = new List<object>();

            list.ToList().ForEach(
                d =>
                {
                    dirList = JsonConvert.DeserializeObject<List<QuoteDirectionViewModel>>(d.Direction);
                    var polygons = new List<object>();
                    dirList.ForEach(point => polygons.Add(new decimal[] { Convert.ToDecimal(point.latitude), Convert.ToDecimal(point.longitude) }));

                    var obj = new { dir = polygons, lookup = d.Quote.Lookup };
                    moves.Add(obj);
                });

            return Json(new { data = moves }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Template()
        {
            return View();
        }

        public ActionResult ViewResponse(Guid id)
        {
            var transactions = new TransactionLogRepository();
            TransactionLog tran = transactions.Get(id);

            return Content(Security.Decrypt(tran.Response));
        }

        public ActionResult ViewRequest(Guid id)
        {
            var transactions = new TransactionLogRepository();
            TransactionLog tran = transactions.Get(id);
            return Content(Security.Decrypt(tran.TransactionStore));
        }

        [HttpGet]
        public ActionResult Vehicles(VehicleSort sort = VehicleSort.Lookup, bool desc = false, string errorMessage = "")
        {
            ViewBag.ErrorMessage = errorMessage;
            ViewBag.Desc = desc;
            ViewBag.Sort = sort;

            var franchiseRepo = new FranchiseRepository();
            var repo = new VehicleRepository();
            var model = new VehicleListModel() { Desc = desc, Sort = sort, Vehicles = repo.GetAll(sort, desc), Franchises = franchiseRepo.GetAll() };

            return View(model);
        }

        [HttpPost]
        public ActionResult RemoveVehicle(string lookup)
        {
            var repo = new VehicleRepository();
            Vehicle vehicle = repo.Get(lookup);
            vehicle.IsArchived = true;
            repo.Save();
            return RedirectToAction("Vehicles");
        }

        [HttpPost]
        public ActionResult AddVehicle(
            Guid franchise,
            string lookup,
            string truckName,
            int? cubicFeet,
            decimal? length,
            string make,
            string model,
            int? year,
            string type,
            string description)
        {
            var repo = new VehicleRepository();

            if (String.IsNullOrEmpty(lookup))
            {
                ModelState.AddModelError("lookup", "Keycode is required");
            }

            if (String.IsNullOrEmpty(truckName))
            {
                ModelState.AddModelError("truckName", "Name is required");
            }

            if (repo.Get(lookup) != null)
            {
                ModelState.AddModelError("lookup", "Keycode must be unique");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", new { errorMessage = "Error adding new truck" });
            }

            var item = new Vehicle()
            {
                FranchiseID = franchise,
                CubicFeet = cubicFeet,
                Lookup = lookup,
                Name = truckName,
                Length = length,
                Make = make,
                Model = model,
                Year = year,
                Type = type,
                Description = description
            };

            repo.Add(item);
            repo.Save();

            return RedirectToAction("Vehicles", "Admin");
        }

        public ActionResult CreditCardResponses()
        {
            var repo = new TransactionLogRepository();
            return View(repo.db.TransactionLogs.OrderByDescending(i => i.Date).Take(250));
        }

        public ActionResult ArchiveAccount(string lookup)
        {
            var repo = new AccountRepository();
            Account account = repo.Get(lookup);
            account.IsArchived = true;
            repo.Save();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnarchiveAccount(string lookup)
        {
            var repo = new AccountRepository();
            Account account = repo.Get(lookup);
            account.IsArchived = false;
            repo.Save();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddEmployeeLogin(Guid? employeeid = null)
        {
            var model = new EmployeeLoginViewModel() { Employees = EmployeeeRepository.GetWithoutLogins(SessionFranchiseID) };

            return View(model);
        }

        [HttpPost]
        public ActionResult AddEmployeeLogin(EmployeeLoginViewModel model)
        {
            return null;
        }

        [HttpPost]
        public ActionResult GetEmployeeProfile(Guid empId)
        {
            var repo = new EmployeeRepository();
            Employee employee = repo.Get(empId);
            return
                Json(
                    new
                    {
                        FirstName = employee.NameFirst,
                        LastName = employee.NameLast,
                        Email = employee.Email != null ? employee.Email.Email : string.Empty,
                        Phone = (employee.PrimaryPhone ?? employee.SecondaryPhone) != null ? (employee.PrimaryPhone ?? employee.SecondaryPhone).Number : ""
                    });
        }

        public ActionResult ProfileAccessMatrix(bool success = false)
        {
            ViewBag.Success = success;
            IEnumerable<Role_App_Rel> profilesacesses = new RoleAppRelationRepository().GetAll();
            return View(profilesacesses);
        }

        [HttpPost]
        public ActionResult ProfileAccessMatrix(string relid, string appid, string roleid, bool ischecked)
        {
            var roleapprelrepo = new RoleAppRelationRepository();
            Role_App_Rel rel = roleapprelrepo.Get(Guid.Parse(relid));
            rel.IsAllowedAccess = ischecked;
            roleapprelrepo.Save(ApplicationType.Crm);
            return new JsonResult()
            {
                Data = new { result = ischecked, message = ischecked ? "Access Right Set Successfully" : "Access Right Removed Successfully" }
            };
        }

        public ActionResult Locations(LocationSort sort = LocationSort.StoreId, bool desc = false)
        {
            IOrderedQueryable<Location> locations = new LocationRepository().GetAll();
            return View(new LocationListModel(locations, sort, desc));
        }

        public ActionResult NewLocation()
        {
            //TODO: Implement this view for new and edit
            return null;
        }

        public ActionResult UserProfiles()
        {
            IEnumerable<aspnet_Roles> roles = new aspnet_RolesRepository().GetAll();
            return View(roles);
        }

        public ActionResult NewUserProfile()
        {
            //TODO: Implement this view for new and edit
            return View(new aspnet_Roles());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewUserProfile(aspnet_Roles model)
        {
            var asprolerepo = new aspnet_RolesRepository();
            var applicationrepo = new ApplicationRepository();
            IEnumerable<Application> apps = applicationrepo.GetAll();

            model.ApplicationId = AspUser.ApplicationId;
            model.LoweredRoleName = model.RoleName;

            asprolerepo.Add(model);
            asprolerepo.Save();

            foreach (Application application in apps)
            {
                model.Role_App_Rel.Add(
                    new Role_App_Rel() { AppId = application.AppId, RoleId = model.RoleId, AppName = application.AppName, RoleName = model.RoleName });
            }

            asprolerepo.Save();
            asprolerepo.ToString();

            return RedirectToAction("UserProfiles");
        }
    }
}