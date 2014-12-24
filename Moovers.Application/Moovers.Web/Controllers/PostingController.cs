using System;
using System.Web.Mvc;
using Business.Enums;
using Business.Repository;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using Business.Utility;
using Moovers.WebModels;
using PostSortColumn = Business.Enums.PostSortColumn;
using System.Collections.Generic;
using System.Linq;
using MooversCRM.Attributes;

namespace MooversCRM.Controllers
{
    //[Authorize]
    [MenuDescription("Accounting")]
    public class PostingController : BaseControllers.SecureBaseController
    {
        // GET: /Posting/
        public ActionResult Index(PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = false, int page = 0, int pageSize = 100)
        {
            var repo = new PostingRepository();
            var unposted = new PagedResult<Business.Interfaces.IPosting>(repo.GetUnposted(SessionFranchiseID, sort, desc), page, pageSize);
            return View(new PostListModel("unposted", unposted, sort, desc, new List<PostSortColumn>() {
                PostSortColumn.Balance,
                PostSortColumn.PostingDate,
                PostSortColumn.Print,
                PostSortColumn.LostDebt
            }));
        }

        [HttpPost]
        public ActionResult MarkIncomplete(string id)
        {
            var repo = new PostingRepository();
            var post = repo.Get(id);
            post.IsComplete = false;
            post.DateCompleted = null;
            post.CompletedBy = null;
            post.Quote.ForceStatusUpdate(AspUserID, QuoteStatus.Scheduled, "Posting marked incomplete");
            repo.Save();
            return RedirectToAction("Post", new { id = post.PostingID });
        }

        public ActionResult Search(string q, PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = false, int page = 0, int pageSize = 100)
        {
            ViewBag.Search = q;
            var repo = new PostingRepository();
            var results = new PagedResult<Business.Interfaces.IPosting>(repo.GetSearch(SessionFranchiseID, q, sort, desc), page, pageSize);
            return View(new PostListModel("unposted", results, sort, desc, new List<PostSortColumn>() {
                PostSortColumn.PostingDate,
                PostSortColumn.Print,
                PostSortColumn.Vehicles,
                PostSortColumn.LostDebt
            }));
        }

        public ActionResult Post(Guid id)
        {
            var repo = new PostingRepository();
            var post = repo.Get(id);

            if (post == null || !post.Quote.CanUserRead(User.Identity.Name))
            {
                return RedirectToAction("Index");
            }

            var employeeRepo = new EmployeeRepository();
            var employees = employeeRepo.GetAllIncludingArchived(post.Quote.FranchiseID);
            var vehicleRepo = new VehicleRepository();
            var vehicles = vehicleRepo.GetAll(post.Quote.FranchiseID);
            var model = new PostModel(employees, post, vehicles);
            return View(model);
        }

        public new ActionResult View(string id, string referrer)
        {
            ViewBag.Referrer = referrer;
            var repo = new PostingRepository();
            var employeeRepo = new EmployeeRepository();
            var vehicleRepo = new VehicleRepository();

            var posting = repo.Get(id);

            if (posting == null || !posting.Quote.CanUserRead(User.Identity.Name))
            {
                return RedirectToAction("Index");
            }

            var employees = employeeRepo.GetAllIncludingArchived(posting.Quote.FranchiseID);
            var vehicles = vehicleRepo.GetAll(posting.Quote.FranchiseID);
            var model = new PostModel(employees, posting, vehicles);
            return View("Post", model);
        }

        public ActionResult Posted(PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = true, int page = 0, int pageSize = 50)
        {
            var repo = new PostingRepository();
            var posted = new PagedResult<Business.Interfaces.IPosting>(repo.GetPosted(SessionFranchiseID, sort, desc), page, pageSize);
            return View(new PostListModel("posted", posted, sort, desc, new List<PostSortColumn>() {
                PostSortColumn.Print,
                PostSortColumn.LostDebt
            }));
        }

        public ActionResult Unpaid(PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = false, int page = 0, int pageSize = 100)
        {
            var repo = new PostingRepository();
            var unpaid = new PagedResult<Business.Interfaces.IPosting>(repo.GetUnpaid(SessionFranchiseID, sort, desc), page, pageSize);
            return View(new PostListModel("unpaid", unpaid, sort, desc, new List<PostSortColumn>() { 
                PostSortColumn.Print
            }));
        }

        [HttpPost]
        public ActionResult MarkAsLostDebt(Guid quoteid)
        {
            var repo = new QuoteRepository();
            var quote = repo.Get(quoteid);
            quote.IsLostDebt = true;
            repo.Save();
            return RedirectToAction("Unpaid");
        }

        public ActionResult Unprinted(DateTime? date, PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = false, int page = 0, int pageSize = 100)
        {
            var repo = new ScheduleRepository();
            date = date ?? DateTime.Now;
            ViewBag.Date = date.Value;
            var unprinted = new PagedResult<Business.Interfaces.IPosting>(repo.GetUnprinted(SessionFranchiseID, date.Value, sort, desc), page, pageSize);
            return View(new PostListModel("unprinted", unprinted, sort, desc, new List<PostSortColumn>() {
                PostSortColumn.Employees,
                PostSortColumn.Vehicles,
                PostSortColumn.Balance,
                PostSortColumn.Price,
                PostSortColumn.PostingDate,
                PostSortColumn.LostDebt
            }));
        }

        public ActionResult Cancelled(PostSortColumn sort = PostSortColumn.ServiceDate, bool desc = false, int page = 0, int pageSize = 100)
        {
            var repo = new PostingRepository();
            var cancelled = new PagedResult<Business.Interfaces.IPosting>(repo.GetCancelled(SessionFranchiseID, sort, desc), page, pageSize);
            return View(new PostListModel("cancelled", cancelled, sort, desc, new List<PostSortColumn>() {
                PostSortColumn.Print,
                PostSortColumn.LostDebt

            }));
        }
    }
}
