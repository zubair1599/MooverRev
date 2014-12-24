// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Payment.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Utility
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Business.FirstData;
    using Business.Interfaces;
    using Business.Models;
    using Business.Repository.Models;

    public enum PaymentTypes
    {
        [Description("Credit Card")]
        CreditCard,

        [Description("Cash")]
        Cash,

        [Description("Cashier's Check")]
        CashierCheck,

        [Description("Personal Check")]
        PersonalCheck,

        [Description("Other")]
        Other
    }

    public class CreditCardPayment
    {
        /// <summary>
        ///     Simple regexes to match various types of credit cards
        /// </summary>
        private static readonly IDictionary<FirstDataCardTypes, Regex> CreditCardExpressions = new Dictionary<FirstDataCardTypes, Regex>
        {
            { FirstDataCardTypes.Visa, new Regex(@"^4[0-9]{12}(?:[0-9]{3})?$") },
            { FirstDataCardTypes.Mastercard, new Regex(@"^5[1-5][0-9]{14}$") },
            { FirstDataCardTypes.Amex, new Regex(@"^3[47][0-9]{13}$") },
            { FirstDataCardTypes.Discover, new Regex(@"^6(?:011|5[0-9]{2})[0-9]{12}$") },
            { FirstDataCardTypes.DinersClub, new Regex(@"^3(?:0[0-5]|[68][0-9])[0-9]{11}$") },
            { FirstDataCardTypes.JCB, new Regex(@"^(?:2131|1800|35\d{3})\d{11}$") }
        };

        private IRepository<TransactionLog> transactionLogRepo;

        private IPaymentRepository paymentRepo;

        public CreditCardPayment()
        {
            transactionLogRepo = new TransactionLogRepository();
            paymentRepo = new PaymentRepository();
        }

        public CreditCardPayment(IRepository<TransactionLog> repo, IPaymentRepository paymentRepo)
        {
            transactionLogRepo = repo;
            this.paymentRepo = paymentRepo;
        }

        public static bool IsValidCard(string card)
        {
            try
            {
                GetCardType(card);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static FirstDataCardTypes GetCardType(string number)
        {
            try
            {
                KeyValuePair<FirstDataCardTypes, Regex> match = CreditCardExpressions.First(i => i.Value.IsMatch(number));
                return match.Key;
            }
            catch (Exception)
            {
                throw new PaymentException("Invalid number");
            }
        }

        /// <summary>
        ///     Requests a TransArmor token from credit card information.
        /// </summary>
        /// <param name="franchise">Franchise -- each franchise has their own Global Gateway credentials</param>
        /// <param name="name">Name on credit card</param>
        /// <param name="cardnumber">Credit card number</param>
        /// <param name="expirationmonth">Credit card expiration month</param>
        /// <param name="expirationyear">Credit card expiration year</param>
        /// <param name="cvv2">Credit card cvv2</param>
        /// <param name="billingZip">Credit card billing zip code</param>
        /// <param name="customerid">Customer ID that will show up in the First Data console (usually Account #)</param>
        public TransactionResult GetCreditCardToken(
            Franchise franchise,
            string name,
            string cardnumber,
            string expirationmonth,
            string expirationyear,
            string cvv2,
            string billingZip,
            string customerid)
        {
            if (expirationyear.Length != 2)
            {
                throw new PaymentException("Expiration Year must be 2 digits");
            }

            if (expirationmonth.Length != 2)
            {
                throw new PaymentException("Expiration Month must be 2 digits");
            }

            FirstDataCardTypes cardType = GetCardType(cardnumber);

            // filter CC types that are not accepted
            if (cardType == FirstDataCardTypes.DinersClub)
            {
                throw new PaymentException("Invalid Card -- Diner's Club");
            }

            if (cardType == FirstDataCardTypes.JCB)
            {
                throw new PaymentException("Invalid Card -- JCB");
            }

            string transactionid = paymentRepo.GetUniqueTransactionID();
            var tran = new Transaction
            {
                ExactID = franchise.PaymentGatewayID,
                Password = franchise.PaymentPassword,
                Card_Number = cardnumber,
                CardType = cardType.GetDescription(),
                Currency = FirstDataCurrency.USDollar.GetDescription(),
                Transaction_Type = FirstDataTransactionTypes.PreAuthorizationOnly.GetDescription(),
                Expiry_Date = expirationmonth + expirationyear,
                CardHoldersName = name,
                VerificationStr2 = cvv2,
                CVD_Presence_Ind = "1",
                ZipCode = billingZip,
                Customer_Ref = customerid,
                Reference_No = transactionid
            };
            var client = new ServiceSoapClient();
            var resp = new TransactionResult();

            try
            {
                resp = client.SendAndCommit(tran);
            }
            catch (Exception e)
            {
                var log2 = new TransactionLog(tran, resp, "GetCreditCardToken", transactionid, customerid);
                transactionLogRepo.Add(log2);
                transactionLogRepo.Save();
                throw new PaymentException(e.Message);
            }

            var log = new TransactionLog(tran, resp, "GetCreditCardToken", transactionid, customerid);
            transactionLogRepo.Add(log);
            transactionLogRepo.Save();

            if (resp.Transaction_Error || !resp.Transaction_Approved)
            {
                if (resp.EXact_Message != "Transaction Normal")
                {
                    throw new PaymentException(resp.EXact_Message);
                }

                if (resp.Bank_Message == "Declined")
                {
                    if (resp.Bank_Resp_Code == "353")
                    {
                        throw new PaymentException("Address verification failure");
                    }
                }

                throw new PaymentException(resp.Bank_Message);
            }

            return resp;
        }

        /// <summary>
        ///     Charges a TransArmor Token
        ///     This does not accept normal CC information, a token must be passed through "GetCreditCardToken"
        /// </summary>
        public Payment ChargeCreditCard(
            Franchise franchise,
            IPayable payable,
            Guid creditCardID,
            string transArmorToken,
            string cardholder,
            string cardType,
            string expiry,
            decimal amount,
            string customerid,
            string transactionid,
            string clientEmail,
            out string errorMessage,
            IRepository paymentRepo,
            out bool successful)
        {
            Payment payment = payable.GetNewPayment();
            payment.Amount = amount;
            payment.CreditCardID = creditCardID;
            payment.Date = DateTime.Now;
            payment.Success = false;
            payment.TransactionID = transactionid;
            payment.AcceptedBy = null;
            payment.FranchiseID = franchise.FranchiseID;
            payable.AddPayment(payment);
            paymentRepo.Save();

            if (!payable.CheckPayment(payment))
            {
                errorMessage = "Transaction not Created before Charging Credit Card";
                successful = false;
                return payment;
            }

            var tran = new Transaction
            {
                ExactID = franchise.PaymentGatewayID,
                Password = franchise.PaymentPassword,
                Currency = FirstDataCurrency.USDollar.GetDescription(),
                CardHoldersName = cardholder,
                CardType = cardType,
                Transaction_Type = FirstDataTransactionTypes.Purchase.GetDescription(),
                Expiry_Date = expiry,
                TransarmorToken = transArmorToken,
                Customer_Ref = customerid,
                Reference_No = transactionid,
                DollarAmount = amount.ToString()
            };

            if (!String.IsNullOrEmpty(clientEmail))
            {
                tran.Client_Email = clientEmail;
            }

            var client = new ServiceSoapClient();
            TransactionResult resp = client.SendAndCommit(tran);

            var log = new TransactionLog(tran, resp, "Purchase", transactionid, customerid);
            transactionLogRepo.Add(log);
            transactionLogRepo.Save();

            bool success = (resp.Transaction_Approved && !resp.Transaction_Error);
            payment.Success = success;
            paymentRepo.Save();

            if (resp.Transaction_Error || !resp.Transaction_Approved)
            {
                if (resp.EXact_Message != "Transaction Normal")
                {
                    errorMessage = resp.EXact_Message;
                }
                else
                {
                    errorMessage = resp.Bank_Message;
                }
            }
            else
            {
                errorMessage = String.Empty;
            }

            successful = success;

            return payment;
        }
    }

    [Serializable]
    public class PaymentException : Exception
    {
        public PaymentException(string message) : base(message)
        {
        }
    }
}