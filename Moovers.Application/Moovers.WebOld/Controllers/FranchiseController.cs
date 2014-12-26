using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Models;
using Business.Repository.Models;
using Moovers.WebModels;

namespace MooversCRM.Controllers
{
    public class FranchiseController : BaseControllers.SecureBaseController
    {
        private MooversCRMEntities db = new MooversCRMEntities();

        //
        // GET: /Franchise/

        public ActionResult Index()
        {
            var repo = new FranchiseRepository();
             
            return View(repo.GetAll());
        }

        //
        // GET: /Franchise/Details/5

        public ActionResult Details(Guid id)
        {
            var repo = new FranchiseRepository();
            var franchise = repo.Get(id);
            if (franchise == null)
            {
                return HttpNotFound();
            }
            return View(franchise);
        }

        //
        // GET: /Franchise/Create

        public ActionResult Create()
        {
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "Street1");
            ViewBag.OldAddressID = new SelectList(db.Addresses, "AddressID", "Street1");
            return View();
        }

        //
        // POST: /Franchise/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Franchise franchise)
        {
            if (ModelState.IsValid)
            {
                var repo = new FranchiseRepository();
                repo.Add(franchise);
                repo.Save();
               
                return RedirectToAction("Index");
            }

            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "Street1", franchise.AddressID);
            ViewBag.OldAddressID = new SelectList(db.Addresses, "AddressID", "Street1", franchise.OldAddressID);
            return View(franchise);
        }

        //
        // GET: /Franchise/Edit/5

        public ActionResult Edit(Guid id)
        {
            var repo = new FranchiseRepository();
            var franchise = repo.Get(id);
            
            if (franchise == null)
            {
                return HttpNotFound();
            }
            var addressModel = new AddressModel(AddressType.Mailing,franchise.Address);
            var editFranchiseModel = new EditFranchiseModel(franchise, addressModel, true);
            return View(franchise);
        }

        //
        // POST: /Franchise/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Franchise franchise)
        {
            if (ModelState.IsValid)
            {
                var repo = new FranchiseRepository();
                repo.Save();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(db.Addresses, "AddressID", "Street1", franchise.AddressID);
            ViewBag.OldAddressID = new SelectList(db.Addresses, "AddressID", "Street1", franchise.OldAddressID);
            return View(franchise);
        }

        //
        // GET: /Franchise/Delete/5

        public ActionResult Delete(Guid id)
        {
            Franchise franchise = db.Franchises.Single(f => f.FranchiseID == id);
            if (franchise == null)
            {
                return HttpNotFound();
            }
            return View(franchise);
        }

        //
        // POST: /Franchise/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Franchise franchise = db.Franchises.Single(f => f.FranchiseID == id);
            db.Franchises.DeleteObject(franchise);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}