using Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Repository;
using Business.Repository.Models;
using Moovers.WebModels;
using Moovers.WebModels.Validators;

namespace MooversCRM.Controllers
{
    public class PaymentController : BaseControllers.SecureBaseController
    {
        private bool AddExistingCardPayment(Business.Interfaces.IPayable payable, Business.Models.RepositoryBase repo, Guid cardid, decimal amount, Guid userid, out string errorMessage, out string transactionID)
        {
            var cardRepo = new Account_CreditCardRepository();
            var card = cardRepo.Get(cardid);

            transactionID = new PaymentRepository().GetUniqueTransactionID();
            if (card == null)
            {
                errorMessage = "Invalid card";
                return false;
            }

            string email = String.Empty;
            var emailAcct = payable.Account.GetEmail(Business.Models.EmailAddressType.Primary);
            
            if (emailAcct != null)
            {
                email = emailAcct.Email;
            }

            var cardpayment = new Business.Utility.CreditCardPayment();
            bool successful;
            var payment = cardpayment.ChargeCreditCard(
                franchise: payable.Franchise,
                payable: payable,
                creditCardID: card.CreditCardID,
                transArmorToken: card.GetCard(),
                cardholder: card.GetCardHolder(),
                cardType: card.CardType,
                expiry: card.GetExpiration(),
                amount: amount,
                customerid: payable.Account.Lookup,
                transactionid: transactionID,
                clientEmail: email,
                paymentRepo: (IRepository)repo,
                errorMessage: out errorMessage,
                successful: out successful
            );

            repo.Save();
            payment.AcceptedBy = userid;
            repo.Save();

            return successful;
        }

        [HttpPost]
        public ActionResult AddPayment(Business.ViewModels.AddPaymentViewModel model)
        {
            var quoteRepo = new QuoteRepository();
            var storageRepo = new StorageWorkOrderRepository();
            var paymentRepo = new PaymentRepository();

            var validator = new AddPaymentValidator();
            var results = validator.Validate(model);
            if (!results.IsValid)
            {
                return Json(new ErrorModel(results));
            }

            var payable = (model.quoteid.HasValue) ? (Business.Interfaces.IPayable)quoteRepo.Get(model.quoteid.Value)
                                                   : storageRepo.Get(model.workOrderID.Value);

            var repo = (payable is Business.Models.Quote) ? (Business.Models.RepositoryBase)quoteRepo : storageRepo;

            if (model.GetPaymentType() != Business.Utility.PaymentTypes.CreditCard)
            {
                var payment = payable.GetNewPayment();
                payment.Amount = model.amount;
                payment.CreditCardID = null;
                payment.Date = DateTime.Now;
                payment.PaymentType = model.GetPaymentType();
                payment.FranchiseID = payable.FranchiseID;
                payment.Success = true;
                payment.TransactionID = paymentRepo.GetUniqueTransactionID();

                if (model.GetPaymentType() == Business.Utility.PaymentTypes.CashierCheck)
                {
                    payment.CheckNumber = model.checkNumber;
                }
                else
                {
                    payment.CheckNumber = model.personalCheckNumber;
                }

                payment.Memo = model.memo;
                payment.AcceptedBy = AspUserID;
                payable.AddPayment(payment);
                repo.Save();

                return Json("Success");
            }

            // if paying w/ a new credit card, add that credit card then charge the card normally.
            if (model.IsNewCard())
            {
                try
                {
                    var account = payable.Account;
                    var card = account.AddCreditCard(payable.Franchise, model);
                    repo.Save();
                    model.method = card.CreditCardID.ToString();
                }
                catch (Business.Utility.PaymentException e)
                {
                    return Json(new ErrorModel("cardnumber", e.Message));
                }
            }

            var cardID = Guid.Parse(model.method);
            string error;
            string transactionid;
            if (AddExistingCardPayment(payable, repo, cardID, model.amount, AspUserID, out error, out transactionid))
            {
                return Json(transactionid);
            }

            if (!String.IsNullOrEmpty(error))
            {
                return Json(new ErrorModel("cardnumber", error));
            }

            return Json(new ErrorModel("cardnumber", "Unable to process payment"));
        }

        public ActionResult CancelPayment(Guid paymentID, string reason)
        {
            var repo = new PaymentRepository();
            var payment = repo.Get(paymentID);
            payment.IsCancelled = true;
            payment.CancellationReason = reason;
            repo.Save();
            return Json(true);
        }
    }
}
