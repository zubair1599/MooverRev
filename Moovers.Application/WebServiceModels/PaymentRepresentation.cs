// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="PaymentRepresentation.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using Business.Models;

namespace WebServiceModels
{
    using System;

    public class PaymentRepresentation
    {
        public Guid? quoteid { get; set; }

        public string method { get; set; }

        public decimal amount { get; set; }
     
        public string name { get; set; }

        public string cardnumber { get; set; }

        public string expirationmonth { get; set; }

        public string expirationyear { get; set; }

        public string cvv2 { get; set; }

        public string billingZip { get; set; }

        public string memo { get; set; }

        public string checkNumber { get; set; }

        public string personalCheckNumber { get; set; }

        public AccountSignature payment_signature { get; set; }
    }

    public class PaymentListRepresentation
    {
        public Guid? quoteid { get; set; }

        public string method { get; set; }

        public decimal amount { get; set; }

        public DateTime payment_date { get; set; }

        public string processed_by { get; set; }

        public string transaction_id { get; set; }

        public bool success { get; set; }
        
        public  string card { get; set; }
         public  string credit_card_last4 { get; set; }
        public  string credit_card_expire { get; set; }


        public string check_no { get; set; }
        public string card_type { get; set; }
    }

    public class CardRepresentation
    {
        
    }
}