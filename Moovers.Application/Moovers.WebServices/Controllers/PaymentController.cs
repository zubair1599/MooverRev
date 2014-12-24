// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="PaymentController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using WebGrease.Css.Extensions;

namespace Moovers.WebServices.Controllers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Business.Interfaces;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;
    using Business.ViewModels;

    using FluentValidation.Results;

    using Moovers.WebModels;
    using Moovers.WebModels.Validators;
    using Moovers.Webservices.Controllers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using WebServiceModels;

    public class PaymentController : ControllerBase
    {
        private readonly ICustomAuthenticationRepository _secretRepository;

        public PaymentController(ICustomAuthenticationRepository secretRepository)
        {
            if (secretRepository == null)
            {
                throw new ArgumentNullException("secretRepository");
            }
            _secretRepository = secretRepository;
        }

        [HttpPost]
        [Route("v1/payment/quote/")]
        public HttpResponseMessage GetQuote(HttpRequestMessage request)
        {
            try
            {
                string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

                var data1 = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;

                Utility.LogHttpRequest(this.GetCurrentUser().UserName, _secretRepository,data1,request.ToString());

                string quotelookup = data1["quote_lookup"].ToString();

                var quoterepo = new QuoteRepository();
                Quote qquote = quoterepo.Get(quotelookup);

                if (qquote.Account_CreditCard != null)
                {
                    var card = new
                    {
                        creditcardid = qquote.Account_CreditCard.CreditCardID,
                        cardtype = qquote.Account_CreditCard.CardType,
                        expiry = qquote.Account_CreditCard.DisplayExpiration(),
                        last4 = qquote.Account_CreditCard.DisplayCard()
                    };

                    var quote = new { quote_id = qquote.QuoteID, card = card };

                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", title = "", message = "card found", data = new { quote = quote } });
                }

                var quote1 = new { quote_id = qquote.QuoteID, card = "" };

                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", title = "", message = "card not found", data = new { quote = quote1 } });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/payment/list/")]
        public HttpResponseMessage GetExistingPayments(HttpRequestMessage request)
        {
            try
            {
                string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

                var data1 =
                    JsonSerializer.Create()
                        .Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;

                var sb = new StringBuilder();
                sb.Append(data1);
                sb.Append(request.ToString());

                _secretRepository.LogRequest(this.GetCurrentUser().UserName, sb);

                string quotelookup = data1["quote_lookup"].ToString();

                var quoterepo = new QuoteRepository();

                var quote = quoterepo.Get(quotelookup);

                if (quote == null) throw new Exception("Quote not found");

                var payments = quote.GetPayments();

                var existingPayments = new List<PaymentListRepresentation>();

                if (payments != null)
                    payments.ForEach(p =>
                    {
                        var pay = new PaymentListRepresentation()
                        {
                            quoteid = quote.QuoteID,
                            transaction_id = p.TransactionID,
                            payment_date = p.Date,
                            method = p.PaymentType.ToString(),
                            amount = p.Amount,
                            processed_by = p.GetProcessedBy() != null ? p.GetProcessedBy().UserName : "",
                            success = !p.IsCancelled,
                            card = p.Account_CreditCard != null ? p.Account_CreditCard.DisplayCard() : "",
                            credit_card_last4 = p.Account_CreditCard != null ? p.Account_CreditCard.GetLast4() : "",
                            credit_card_expire =
                                p.Account_CreditCard != null ? p.Account_CreditCard.GetExpiration() : "",
                            check_no = p.CheckNumber,
                            card_type = p.Account_CreditCard != null ? p.Account_CreditCard.CardType : ""
                        };
                        existingPayments.Add(pay);
                    });

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        status = "success",
                        title = "",
                        message = "card not found",
                        data = new {payments = existingPayments}
                    });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/payment/authorize/")]
        public HttpResponseMessage AuthorizePayment(HttpRequestMessage request)
        {
            try
            {
                string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

                var data1 = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;

                var sb = new StringBuilder();
                sb.Append(data1);
                sb.Append(request.ToString());

                _secretRepository.LogRequest(this.GetCurrentUser().UserName, sb);

                object paymentjson = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data1["payment_info"].ToString())));
                PaymentRepresentation payment = null;

                if (paymentjson != null)
                {
                    payment = JsonConvert.DeserializeObject(paymentjson.ToString(), typeof(PaymentRepresentation)) as PaymentRepresentation;
                }

                if (payment.payment_signature != null)
                {
                    payment.payment_signature.SignatureTime = DateTime.Now;
                    // SetTime(data1, payment);                    
                }

                string message = AddPayment(ConvertPayemtRepresetation(payment));

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { status = "success", title = "", message = "data updated successfully", data = new { updated = true, resultmessage = message } });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        private static void SetTime(JObject data1, PaymentRepresentation payment)
        {
            var inner = data1["payment_info"].Value<JObject>();
            if (inner != null)
            {
                var signature = inner["payment_signature"].Value<JToken>();
                if (signature != null)
                {
                    var time = signature["signature_time"].Value<JToken>();

                    if (payment != null && payment.payment_signature != null)
                    {
                        payment.payment_signature.SignatureTime = Convert.ToDateTime(time);
                    }
                }
            }
        }

        private HttpResponseMessage GetFaultMessage(Exception ex)
        {
            int line = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    status = "Failed",
                    title = ex.Message + ":inner exception: " + ex.InnerException,
                    message = ex.StackTrace,
                    lineno = line,
                    data = new { updated = false }
                });
        }

        private AddPaymentViewModel ConvertPayemtRepresetation(PaymentRepresentation payment)
        {
            var paymentviewmodel = new AddPaymentViewModel
            {
                quoteid = payment.quoteid,
                method = payment.method,
                amount = payment.amount,
                name = payment.name,
                cardnumber = payment.cardnumber,
                expirationmonth = payment.expirationmonth,
                expirationyear = payment.expirationyear,
                cvv2 = payment.cvv2,
                billingZip = payment.billingZip,
                memo = payment.memo,
                checkNumber = payment.checkNumber,
                personalCheckNumber = payment.personalCheckNumber,
                PaymentSignature =  payment.payment_signature               
            };

            return paymentviewmodel;
        }

        private string AddPayment(AddPaymentViewModel model)
        {
            var quoteRepo = new QuoteRepository();
            var paymentRepo = new PaymentRepository();

            var validator = new AddPaymentValidator();
            ValidationResult results = validator.Validate(model);
            if (!results.IsValid)
            {
                throw new PaymentException((new ErrorModel(results)).SerializeToJson());
            }

            var payable = (IPayable)quoteRepo.Get(model.quoteid.Value);

            var repo = (RepositoryBase)quoteRepo;

            if (model.GetPaymentType() != PaymentTypes.CreditCard)
            {
                Payment payment = payable.GetNewPayment();
                payment.Amount = model.amount;
                payment.CreditCardID = null;
                payment.Date = DateTime.Now;
                payment.PaymentType = model.GetPaymentType();
                payment.FranchiseID = payable.FranchiseID;
                payment.Success = true;
                payment.TransactionID = paymentRepo.GetUniqueTransactionID();

                if (model.GetPaymentType() == PaymentTypes.CashierCheck)
                {
                    payment.CheckNumber = model.checkNumber;
                }
                else
                {
                    payment.CheckNumber = model.personalCheckNumber;
                }

                payment.Memo = model.memo;
                payment.AcceptedBy = GetCurrentUser().UserId;
                payable.AddPayment(payment);
                if (model.PaymentSignature != null)
                {
                    (payment as QuotePayment).AccountSignature = model.PaymentSignature;
                    (payment as QuotePayment).AccountSignature.Account = (payment as QuotePayment).Quote.Account;
                    //payment.QuotePayments.AccountSignature = model.PaymentSignature;
                    //payment.QuotePayments.AccountSignature.Account = payment.QuotePayments.Quote.Account;
                }
                repo.Save();

                return ("Success");
            }

            // if paying w/ a new credit card, add that credit card then charge the card normally.
            if (model.IsNewCard())
            {
                try
                {
                    Account account = payable.Account;
                    Account_CreditCard card = account.AddCreditCard(payable.Franchise, model);
                    repo.Save();
                    model.method = card.CreditCardID.ToString();
                }
                catch (PaymentException e)
                {
                    throw new PaymentException((new ErrorModel("cardnumber", e.Message)).SerializeToJson());
                }
            }

            Guid cardID = Guid.Parse(model.method);
            string error;
            string transactionid;
            if (AddExistingCardPayment(payable, repo, cardID, model.amount,model.PaymentSignature, GetCurrentUser().UserId, out error, out transactionid))
            {
                return (transactionid).ToString();
            }

            if (!String.IsNullOrEmpty(error))
            {
                throw new PaymentException((new ErrorModel("cardnumber", error)).SerializeToJson());
            }

            throw new PaymentException((new ErrorModel("cardnumber", "Unable to process payment")).SerializeToJson());
        }

        private bool AddExistingCardPayment(
            IPayable payable,
            RepositoryBase repo,
            Guid cardid,
            decimal amount,
            AccountSignature sign,
            Guid userid,
            out string errorMessage,
            out string transactionID)
        {
            var cardRepo = new Account_CreditCardRepository();
            Account_CreditCard card = cardRepo.Get(cardid);

            transactionID = new PaymentRepository().GetUniqueTransactionID();
            if (card == null)
            {
                errorMessage = "Invalid card";
                return false;
            }

            string email = String.Empty;
            EmailAddress emailAcct = payable.Account.GetEmail(EmailAddressType.Primary);

            if (emailAcct != null)
            {
                email = emailAcct.Email;
            }

            var cardpayment = new CreditCardPayment();
            bool successful;
            Payment payment = cardpayment.ChargeCreditCard(
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
                successful: out successful);
            if (sign != null)
            {
                payment.QuotePayments.AccountSignature = sign;
                payment.QuotePayments.AccountSignature.Account = payment.QuotePayments.Quote.Account;
            }
            payment.QuotePayments.AccountSignature = sign;
            repo.Save();
            payment.AcceptedBy = userid;
            
            repo.Save();

            return successful;
        }
    }
}