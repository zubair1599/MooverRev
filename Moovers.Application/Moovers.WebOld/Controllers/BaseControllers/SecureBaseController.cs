// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="SecureBaseController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers.BaseControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using Moovers.WebModels;

    using Roles = Business.Roles;

#if RELEASE
    [RequireHttps]
#endif

    [Authorize]
    public abstract class SecureBaseController : BaseController
    {
        public bool? allowedInController = false;

        public bool IsCorporateUser
        {
            get { return Roles.IsCorporateUser(System.Web.Security.Roles.GetRolesForUser()); }
        }

        public bool IsFranchiseUser
        {
            get { return !IsCorporateUser; }
        }

        /// <summary>
        ///     For users with multiple franchises, this will return their current selected franchise
        ///     For users with a single franchise, this will return their franchise.
        /// </summary>
        protected Guid SessionFranchiseID
        {
            get
            {
                if (AspUser.HasMultipleFranchises())
                {
                    if (_sessionfranchiseid == null)
                    {
                        _sessionfranchiseid = new FranchiseRepository().GetStorage().FranchiseID;
                    }

                    return _sessionfranchiseid.Value;
                }

                //if (AspUser.HasMultipleFranchises())
                //{
                //    if (Session["franchiseid"] == null)
                //    {
                //        // if a user can access multiple franchises, we default to Kansas City
                //        Session["franchiseid"] = new FranchiseRepository().GetStorage().FranchiseID;
                //    }

                //    return (Guid)Session["franchiseid"];
                //}

                return AspUser.GetSingleFranchise().FranchiseID;
            }

            set
            {
                //Session["franchiseid"] = value;
                _sessionfranchiseid = value;
            }
        }

        private Guid? _sessionfranchiseid;

        protected Guid WebQuoteUserID
        {
            get { return (Guid)Membership.GetUser(General.WebQuoteUser).ProviderUserKey; }
        }

        protected bool IsAdministrator
        {
            get { return Roles.IsInRole(User.Identity.Name, Roles.CorporateRoles.Administrator); }
        }

        protected Guid AspUserID
        {
            get { return (Guid)Membership.GetUser(User.Identity.Name).ProviderUserKey; }
        }

        protected aspnet_User AspUser
        {
            get
            {
                var repo = new aspnet_UserRepository();
                return repo.Get(AspUserID);
            }
        }

        protected aspnet_Users_Profile UserProfile
        {
            get { return AspUser.aspnet_Users_Profile.FirstOrDefault(); }
        }

        protected IEnumerable<string> UserFranchises
        {
            get { return AspUser.Franchise_aspnetUser.Select(f => f.Franchise.Name); }
        }

        private bool ShowStorage
        {
            get { return IsCorporateUser; }
        }

        private bool ShowLeads
        {
            get { return true; }
        }

        private bool ShowAccounts
        {
            get
            {
                // all users can see accounts to some extent, permissions are on a per-account basis
                return true;
            }
        }

        private bool ShowQuotes
        {
            get
            {
                // all users can see quotes to some extent, permissions are on a per-quote basis
                return true;
            }
        }

        private bool ShowSchedule
        {
            get
            {
                // all users can see quotes to some extent, permissions are on a per-quote basis
                return true;
            }
        }

        private bool ShowCases
        {
            get { return true; }
        }

        private bool ShowEmployees
        {
            get
            {
                var allowedRoles = new object[]
                {
                    Roles.CorporateRoles.Administrator, Roles.CorporateRoles.CallCenterSupervisor, Roles.CorporateRoles.CallCenterAgent,
                    Roles.CorporateRoles.HumanResources, Roles.CorporateRoles.Claims, Roles.FranchiseRoles.Dispatch, Roles.FranchiseRoles.Manager,
                    Roles.FranchiseRoles.Sales
                };

                return Roles.IsInRole(User.Identity.Name, allowedRoles);
            }
        }

        private bool ShowTemplate
        {
            get { return true; }
        }

        private bool ShowAdmin
        {
            get { return Roles.IsInRole(User.Identity.Name, Roles.CorporateRoles.Administrator); }
        }

        private bool ShowPosting
        {
            get { return true; }
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            string mainRole = string.Empty;
            bool? allowed = false;
            string requestedUrl = (filterContext.Result != null) ? filterContext.Result.ToString() : "";
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                string[] userRoles = System.Web.Security.Roles.GetRolesForUser(User.Identity.Name);
                if (userRoles != null && userRoles.Count() > 0)
                {
                    ControllerBase controller = filterContext.Controller;
                    string controllerName = controller.ControllerContext.RouteData.Values["controller"].ToString();
                    if (!controllerName.Equals("Home"))
                    {
                        foreach (string role in userRoles)
                        {
                            if (allowed == true)
                            {
                                break;
                            }
                            mainRole = role;
                            IEnumerable<Role_App_Rel> profilesacesses = new RoleAppRelationRepository().GetAll();

                            IEnumerable<Role_App_Rel> list = profilesacesses.Where(m => m.AppName.Equals(controllerName)).ToList();

                            if (list.Any())
                            {
                                foreach (Role_App_Rel item in list)
                                {
                                    if (item.AppName.Equals(controllerName) && item.RoleName.Equals(mainRole) && item.IsAllowedAccess)
                                    {
                                        allowed = true;
                                        break;
                                    }
                                    allowed = false;
                                }
                            }
                            else if (list == null || list.Count() == 0)
                            {
                                allowed = null;
                            }
                        }
                        if (allowed == false)
                        {
                            filterContext.Result = new RedirectResult("~/Public/NotAuthorized");
                            //new HomeController().Index("Access Deined");
                        }
                        allowedInController = allowed;
                    }
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Login");
            }

            base.OnAuthorization(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!AspUser.HasMultipleFranchises())
            {
                SessionFranchiseID = AspUser.GetSingleFranchise().FranchiseID;
            }

            ViewBag.IsDevelopment = IsDevelopment;
            ViewBag.IsDevelopmentDB = IsDevelopmentDB;

            ViewBag.ParentMenu = ControllerContext.RouteData.Values["Controller"].ToString().ToLower();
            ViewBag.WebQuoteUserID = WebQuoteUserID;
            ViewBag.ShowStorage = ShowStorage;
            ViewBag.SubMenu = ControllerContext.RouteData.Values["Action"];
            ViewBag.ShowAccounts = ShowAccounts;
            ViewBag.ShowQuotes = ShowQuotes;
            ViewBag.ShowSchedule = ShowSchedule;
            ViewBag.ShowCases = ShowCases;
            ViewBag.ShowEmployees = ShowEmployees;
            ViewBag.ShowTemplate = ShowTemplate;
            ViewBag.ShowAdmin = ShowAdmin;
            ViewBag.ShowLeads = ShowLeads;
            ViewBag.ShowPosting = ShowPosting;
            ViewBag.MaxTruckWeight = Quote.MaxTruckWeight;
            ViewBag.MaxTruckCubicFeet = Quote.MaxTruckCubicFeet;

            ViewBag.IsAdministrator = IsAdministrator;
            ViewBag.MaxPriceDiscount = QuoteRepository.MaxPriceDiscount;

            ViewBag.HasMultipleFranchises = AspUser.HasMultipleFranchises();
            ViewBag.SingleFranchise = (AspUser.HasMultipleFranchises()) ? default(Franchise) : AspUser.GetSingleFranchise();

            var franchiseRepo = new FranchiseRepository();
            ViewBag.SessionFranchiseID = SessionFranchiseID;
            ViewBag.SessionFranchise = franchiseRepo.Get(SessionFranchiseID);
            ViewBag.AllFranchises = franchiseRepo.GetAll();

            ViewBag.DestinationMultiplier = QuotePricingModel.DestinationMultiplier;
            ViewBag.MaxHourlySourceTime = QuoteRepository.MaxHourlySourceTime;
            ViewBag.MaxHourlyTravelTime = QuoteRepository.MaxHourlyTravelTime;

            ViewBag.AbsoluteUrl = AbsoluteUrl;
            ViewBag.UserID = AspUserID;
            ViewBag.UserProfile = UserProfile;
            ViewBag.UserFranchises = UserFranchises;
            if (IsDevelopment)
            {
                ViewBag.JavascriptFiles = GetJavascriptFiles();
                ViewBag.ScreenCssFiles = GetScreenCssFiles();
                ViewBag.PrintCssFiles = GetPrintCssFiles();
            }

            base.OnActionExecuted(filterContext);
        }
    }
}