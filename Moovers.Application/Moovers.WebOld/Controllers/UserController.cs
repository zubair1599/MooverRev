using Business.Repository.Models;
using Business.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Moovers.WebModels;

namespace MooversCRM.Controllers
{
    public class UserController : BaseControllers.SecureBaseController
    {
        public ActionResult ViewProfile(Guid id)
        {
            var repo = new aspnet_UserRepository();
            var model = repo.GetNonCachedUser(id);
            ViewBag.ServerPath = System.Configuration.ConfigurationManager.AppSettings["PictureURL"];
            return View(new EditUserModel(model.aspnet_Users_Profile.FirstOrDefault()));
        }

        public ActionResult ChangePassword(Guid id)
        {
            var repo = new aspnet_UserRepository();
            var model = repo.Get(id);
            return View(new EditUserModel(model.aspnet_Users_Profile.FirstOrDefault()));
        }

        [HttpPost]
        public ActionResult ChangePassword(Guid id, EditUserModel model)
        {
            var repo = new aspnet_UserRepository();

            var user = repo.Get(id);
            var memuser = Membership.GetUser(user.UserName);
            if (!Membership.ValidateUser(memuser.UserName, model.OldPassword))
            {
                ModelState.AddModelError("CustomErrors", "Incorrect Old Password");
            }
            if (model.ConfirmPassword != model.Password)
            {
                ModelState.AddModelError("CustomErrors", "Please ensure your passwords match");
            }

            if (ModelState.IsValid)
            {
                if (model.Password != AccountEditModel.UnchangedPasswordText)
                {
                    try
                    {
                        var oldPass = memuser.ResetPassword();
                        memuser.ChangePassword(oldPass, model.Password);
                        memuser.UnlockUser();

                        Membership.UpdateUser(memuser);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("CustomErrors", "Please ensure your password has a letter, a number, and a symbol");
                    }
                }
            }
            else
            {
                return View(model);
            }
            return RedirectToAction("ViewProfile", new { id = id });
        }


        [HttpPost]
        public ActionResult UploadNewPhoto(Guid userID, HttpPostedFileBase uploadFile)
        {
            if (uploadFile.ContentLength > 0)
            {
                string filePath = Path.Combine(HttpContext.Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["PictureURL"]),
                                               Path.GetFileName(uploadFile.FileName));
                uploadFile.SaveAs(filePath);

                var repo = new aspnet_Users_ProfileRepository();
                var user = repo.Get(userID);
                user.PictureUrl = uploadFile.FileName;
                repo.Save();
            }
            return RedirectToAction("ViewProfile", new { id = userID });
        }
    }
}
