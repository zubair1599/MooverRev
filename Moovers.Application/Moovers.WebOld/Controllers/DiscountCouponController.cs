// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="DiscountCouponController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Web.Mvc;

    using Business.Models;
    using Business.Repository;

    //[Authorize(Roles = "Administrator")]
    public class DiscountCouponController : BaseControllers.SecureBaseController
    {
        private MooversCRMEntities db = new MooversCRMEntities();

        //
        // GET: /DiscountCoupon/

        public ActionResult Index()
        {
            return View(db.DiscountCoupons.ToList());
        }

        //
        // GET: /DiscountCoupon/Details/5

        public ActionResult Details(Guid id)
        {
            DiscountCoupon discountcoupon = db.DiscountCoupons.Single(d => d.DiscountCouponId == id);
            if (discountcoupon == null)
            {
                return HttpNotFound();
            }
            return View(discountcoupon);
        }

        //
        // GET: /DiscountCoupon/Create

        public ActionResult Create()
        {
            return View(new DiscountCoupon());
        }

        //
        // POST: /DiscountCoupon/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DiscountCoupon discountcoupon)
        {
            if (ModelState.IsValid)
            {
                var repo = new DiscountCouponRepository();
                repo.Add(discountcoupon);
                repo.Save();
                return RedirectToAction("Index");
            }

            return View(discountcoupon);
        }

        //
        // GET: /DiscountCoupon/Edit/5

        public ActionResult Edit(Guid id)
        {
            DiscountCoupon discountcoupon = db.DiscountCoupons.Single(d => d.DiscountCouponId == id);
            if (discountcoupon == null)
            {
                return HttpNotFound();
            }
            return View(discountcoupon);
        }

        //
        // POST: /DiscountCoupon/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DiscountCoupon discountcoupon)
        {
            if (ModelState.IsValid)
            {
                db.DiscountCoupons.Attach(discountcoupon);
                db.ObjectStateManager.ChangeObjectState(discountcoupon, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(discountcoupon);
        }

        //
        // GET: /DiscountCoupon/Delete/5

        public ActionResult Delete(Guid id)
        {
            DiscountCoupon discountcoupon = db.DiscountCoupons.Single(d => d.DiscountCouponId == id);
            if (discountcoupon == null)
            {
                return HttpNotFound();
            }
            return View(discountcoupon);
        }

        //
        // POST: /DiscountCoupon/Delete/5

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            DiscountCoupon discountcoupon = db.DiscountCoupons.Single(d => d.DiscountCouponId == id);
            db.DiscountCoupons.DeleteObject(discountcoupon);
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