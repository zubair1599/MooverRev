// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="DiscountCouponRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;

    public class DiscountCouponRepository : RepositoryBase<DiscountCoupon>
    {
        public override DiscountCoupon Get(Guid id)
        {
            return db.DiscountCoupons.FirstOrDefault(coupon => coupon.DiscountCouponId == id);
        }

        public DiscountCoupon GetByCouponCode(string code)
        {
            return db.DiscountCoupons.FirstOrDefault(coupon => coupon.CouponCode == code);
        }

        public IEnumerable<DiscountCoupon> GetAll()
        {
            return db.DiscountCoupons.ToList();
        }
    }
}