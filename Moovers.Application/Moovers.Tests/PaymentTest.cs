using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Business.Models;

namespace Moovers.Tests
{
    [TestClass]
    public class PaymentTest
    {
        private Business.Models.Franchise GetFranchise()
        {
            return new Business.Models.Franchise() {
                FranchiseID = new Guid("da5605a7-ce5c-4253-934e-7f7fa72ce12d"),
                Name = "TEST MOOVERS FRANCHISE",
                PaymentGatewayID = "AD0650-06",
                PaymentPassword = "029t580c"
            };
        }

        private static string GetTestCustomer()
        {
            return "TESTCUST";
        }

        private static string GetTestCardName()
        {
            return "John Doe";
        }
        
        private static string GetDevelopmentEmail()
        {
            return Business.Utility.Email.DevelopmentEmail;
        }

        private Business.FirstData.TransactionResult GetCardToken(string card, string month, string year)
        {
            var tranRepo = new Repository.TestTransactionLogRepository();
            var paymentRepo = new Repository.TestPaymentRepository();
            var payment = new Business.Utility.CreditCardPayment(tranRepo, paymentRepo);
            return payment.GetCreditCardToken(this.GetFranchise(), GetTestCardName(), card, month, year, String.Empty, String.Empty, GetTestCustomer());
        }

        /// <summary>
        /// Ensure when we make a CC Transaction, it's properly logged in all repositories
        /// </summary>
        //[TestMethod]
        public void TestTransactionLog()
        {
            var tranRepo = new Repository.TestTransactionLogRepository();
            var paymentRepo = new Repository.TestPaymentRepository();
            var ccpayment = new Business.Utility.CreditCardPayment(tranRepo, paymentRepo);

            var card = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.VISA);
            var exp = DateTime.Now.AddMonths(1);
            var token = ccpayment.GetCreditCardToken(
                this.GetFranchise(),
                GetTestCardName(),
                card,
                exp.Month.ToString("00"),
                exp.Year.ToString("00").Substring(2),
                "", 
                "",
                GetTestCustomer()
            );

            Assert.AreEqual(tranRepo.GetAll().Count(), 1);

            var quote = new Business.Models.Quote();
            var tranid = paymentRepo.GetUniqueTransactionID();
            string errors;
            bool successful;

            var payment = ccpayment.ChargeCreditCard(
                this.GetFranchise(),
                quote,
                Guid.NewGuid(),
                token.TransarmorToken,
                GetTestCardName(),
                "Visa",
                exp.Month.ToString("00") + exp.Year.ToString("00").Substring(2),
                .01m,
                GetTestCustomer(),
                tranid,
                GetDevelopmentEmail(),
                out errors,
                paymentRepo,
                out successful
            );

            Assert.IsNotNull(paymentRepo.GetByTransactionID(tranid));
            Assert.AreEqual(tranRepo.GetAll().Count(), 2);
            Assert.IsTrue(successful);
            Assert.AreEqual(errors, String.Empty);
            Assert.AreEqual(payment.Amount, .01m);
            Assert.AreEqual(quote.GetPayments().First().PaymentID, payment.PaymentID);
            Assert.IsTrue(payment.Success);

        }

        /// <summary>
        /// Test master card credit card.
        /// </summary>
        [TestMethod]
        public void TestMastercard()
        {
            var exp = DateTime.Now.AddMonths(1);
            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.MASTERCARD);
            var resp = this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));
            Assert.IsTrue(resp.Transaction_Approved);
        }

        /// <summary>
        /// Test an expired mastercard.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Business.Utility.PaymentException))]
        public void TestExpired()
        {
            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.MASTERCARD);
            var exp = DateTime.Now.AddMonths(-1);
            this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));

            // get card token should throw an exception
            Assert.Fail();
        }

        /// <summary>
        /// Test a visa card
        /// </summary>
        [TestMethod]
        public void TestVisa()
        {
            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.VISA);
            var exp = DateTime.Now.AddMonths(1);
            var resp = this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));
            Assert.IsTrue(resp.Transaction_Approved);
        }

        /// <summary>
        /// Test an american express card
        /// </summary>
        [TestMethod]
        public void TestAmex()
        {
            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.AMEX);
            var exp = DateTime.Now.AddMonths(1);
            var resp = this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));

            Assert.IsTrue(resp.Transaction_Approved);
        }

        /// <summary>
        /// Test a JCB credit card - we don't accept JCB, so this should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Business.Utility.PaymentException))]
        public void TestJcb()
        {
            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.JCB_16);
            var exp = DateTime.Now.AddMonths(1);
            this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));

            // get card token should throw an exception
            Assert.Fail();
        }

        /// <summary>
        /// Test an invalid, random credit card #
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Business.Utility.PaymentException))]
        public void TestRandom()
        {
            var cc = Utility.RandomCreditCardNumberGenerator.GetInvalidNumber();
            var exp = DateTime.Now.AddMonths(1);
            this.GetCardToken(cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2));

            // get card token should throw an exception
            Assert.Fail();
        }

         /// <summary>
         /// Charge a newly generated TransArmor token
         /// </summary>
        [TestMethod]
        public void TestTransArmorCharge()
        {
            var tranRepo = new Repository.TestTransactionLogRepository();
            var paymentRepo = new Repository.TestPaymentRepository();
            var ccpayment = new Business.Utility.CreditCardPayment(tranRepo, paymentRepo);

            var cc = Utility.RandomCreditCardNumberGenerator.GetNumber(Utility.CreditCardType.VISA);
            var exp = DateTime.Now.AddMonths(1);

            var resp = ccpayment.GetCreditCardToken(this.GetFranchise(), GetTestCardName(), cc, exp.Month.ToString("00"), exp.Year.ToString("00").Substring(2), "", "", GetTestCustomer());
            var quote = new Business.Models.StorageWorkOrder();
            string errors;
            bool success;
            var payment = ccpayment.ChargeCreditCard(
                this.GetFranchise(),
                quote,
                Guid.NewGuid(),
                resp.TransarmorToken,
                GetTestCardName(),
                "Visa",
                exp.Month.ToString("00") + exp.Year.ToString("00").Substring(2),
                .01m,
                GetTestCustomer(),
                "TESTTRAN",
                GetDevelopmentEmail(),
                out errors,
                paymentRepo,
                out success
            );

            Assert.AreEqual(errors, String.Empty);

            Assert.IsTrue(success);
            Assert.AreEqual(errors, String.Empty);
            Assert.AreEqual(payment.Amount, .01m);
            Assert.AreEqual(quote.GetPayments().First().PaymentID, payment.PaymentID);
            Assert.IsTrue(payment.Success);
        }

        ///// <summary>
        ///// Charge a credit card w/o getting a TransArmor token
        ///// </summary>
        [TestMethod]
        public void TestCardCharge()
        {
            var cc = "4111111111111111";
            var exp = DateTime.Now.AddMonths(1);
            string errors;

            var repo = new Repository.TestTransactionLogRepository();
            var paymentRepo = new Repository.TestPaymentRepository();
            var ccpayment = new Business.Utility.CreditCardPayment(repo, paymentRepo);

            bool success;
            var quote = new Business.Models.StorageWorkOrder();
            var tranid = "TESTTRAN";

            var payment = ccpayment.ChargeCreditCard(
                this.GetFranchise(),
                quote,
                Guid.NewGuid(),
                cc,
                GetTestCardName(),
                "Visa",
                exp.Month.ToString("00") + exp.Year.ToString("00").Substring(2),
                .01m,
                GetTestCustomer(),
                tranid,
                GetDevelopmentEmail(),
                out errors,
                paymentRepo,
                out success
            );

            paymentRepo.Add(payment);

            Assert.IsTrue(success);
            Assert.AreEqual(errors, String.Empty);
            Assert.AreEqual(payment.Amount, .01m);
            Assert.AreEqual(quote.GetPayments().First().PaymentID, payment.PaymentID);
            Assert.IsTrue(payment.Success);
            Assert.IsNotNull(paymentRepo.GetByTransactionID(tranid));
        }
    }
}
