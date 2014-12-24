// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="IPayable.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Business.Models;

    public interface IPayable
    {
        Account Account { get; }

        Guid ID { get; }

        Franchise Franchise { get; }

        Guid FranchiseID { get; }

        Account_CreditCard Account_CreditCard { get; }

        DateTime? DateScheduled { get; }

        IEnumerable<Payment> GetPayments();
        decimal GetTotalDue();

        decimal GetTotalPayments();

        decimal GetBalance();

        Payment GetNewPayment();

        void AddPayment(Payment payment);

        bool CheckPayment(Payment p);
    }
}