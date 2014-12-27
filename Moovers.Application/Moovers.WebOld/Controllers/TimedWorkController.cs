using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Repository;
using Business.Repository.Models;
using Business.Utility;
using System.Data.Objects.SqlClient;
using Moovers.WebModels;
using System.Text;
using Business.Models;

namespace MooversCRM.Controllers
{
    /// <summary>
    /// Actions intended to run outside a standard web request
    /// 
    /// These actions are executed on the servers TaskScheduler using a standard wget request
    /// </summary>
    public class TimedWorkController : BaseControllers.NonSecureBaseController
    {
        private static readonly object leadLock = new object();

        private static readonly object storageLock = new object();

        private static readonly object quoteLock = new object();

        private static QuoteRepository quoteRepo { get; set; }

        private static aspnet_UserRepository userRepo { get; set; }

        private static LeadRepository leadRepo { get; set; }

        private static FranchiseRepository franchiseRepo { get; set; }

        private static StorageWorkOrderRepository storageRepo { get; set; }

        private static FileRepository fileRepo { get; set; }

        private static ScheduleConfirmationRepository confirmRepo { get; set; }

        public TimedWorkController()
        {
            quoteRepo = new QuoteRepository();
            userRepo = new aspnet_UserRepository();
            leadRepo = new LeadRepository();
            franchiseRepo = new FranchiseRepository();
            storageRepo = new StorageWorkOrderRepository();
            fileRepo = new FileRepository();
            confirmRepo = new ScheduleConfirmationRepository();
        }

        /// <summary>
        /// Parse e-mail leads.
        /// </summary>
        /// <returns></returns>
        public ActionResult ParseLeads()
        {
            lock (leadLock)
            {
                var leadManager = new LeadManager();
                using (var remoteLeadRepo = new GmailLeadRepository())
                {
                    foreach (var lead in leadManager.GetLeads(remoteLeadRepo))
                    {
                        leadRepo.Add(lead);
                    }

                    leadRepo.Save();
                    return Json(String.Empty, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Send unbooked moves an e-mail several days before their move date, requesting them to call the Moovers office.
        /// </summary>
        /// <returns></returns>
        public ActionResult SendUnbookedQuoteEmails()
        {
            lock (quoteLock)
            {
                var moveDateAfter = DateTime.Today.AddDays(5);
                var moveDateBefore = DateTime.Today.AddDays(6);

                var toEmail = quoteRepo.GetUnscheduled().Where(i => i.BookEmailsSent == 0 && i.MoveDate >= moveDateAfter && i.MoveDate < moveDateBefore).ToList().Take(1);
                toEmail = toEmail.Where(i => i.Created <= DateTime.Now.AddDays(-1));

                foreach (var item in toEmail)
                {
                    item.BookEmailsSent++;
                    quoteRepo.Save();

                    var model = new Business.ViewModels.EmailModel() {
                        To = item.Account.GetEmail(Business.Models.EmailAddressType.Primary).Email,
                        From = item.AccountManager.aspnet_Membership.Email,
                        Message = String.Empty,
                        Franchise = item.Franchise,
                        AccountManager = item.AccountManager.aspnet_Users_Profile.FirstOrDefault(),
                        Account = item.Account
                    };

                    var body = RenderViewToString("Emails/UnbookedEmail", model);
                    Email.SendLoggedEmail(
                        repo: quoteRepo,
                        obj: item,
                        to: model.To,
                        from: model.From,
                        subject: "Book your move with Moovers",
                        body: body,
                        category: EmailCategory.UnbookedQuote,
                        isMarketing: true,

                        ccs: Enumerable.Empty<string>(),
                        bccs: Enumerable.Empty<string>(),
                        files: Enumerable.Empty<Business.Models.File>()
                    );
                }

                return Json(String.Join(",", toEmail.Select(i => i.Lookup)), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// When leads are received, send the customer an e-mail from a random sales person.
        /// </summary>
        /// <returns></returns>
        public ActionResult SendLeadEmails()
        {
            lock (leadLock)
            {
                var defaultUser = userRepo.GetRandomSalesPerson().aspnet_Users_Profile.FirstOrDefault();

                // initial lead emails
                var initialEmails = leadRepo.GetWithEmailsSent(DateTime.Today, DateTime.Now, 0).Take(1).ToList();
                var secondEmails = leadRepo.GetWithEmailsSent(DateTime.Today.AddDays(-4), DateTime.Today.AddDays(-3), 1).Take(1).ToList();

                var first = new List<string>();
                var second = new List<string>();

                foreach (var lead in initialEmails)
                {
                    var json = lead.GetLeadJson();
                    var tmpAccount = json.GetAccount();
                    var model = new Business.ViewModels.EmailModel() {
                        To = json.Email,
                        From = defaultUser.aspnet_Users.aspnet_Membership.Email,
                        Franchise = lead.Franchise,
                        Message = String.Empty,
                        AccountManager = defaultUser,
                        Account = tmpAccount
                    };
                    var body = RenderViewToString("Emails/LeadInitial", model);
                    Email.SendMarketingEmail(model.To, model.From, "Ready to get Mooving with Moovers?", body, EmailCategory.InitialLeadContact, lead.LeadID);
                    lead.EmailsSent++;
                    leadRepo.Save();

                    first.Add(lead.Email);
                }

                foreach (var lead in secondEmails)
                {
                    var json = lead.GetLeadJson();
                    var tmpAccount = json.GetAccount();
                    var model = new Business.ViewModels.EmailModel() {
                        To = json.Email,
                        From = defaultUser.aspnet_Users.aspnet_Membership.Email,
                        Message = String.Empty,
                        AccountManager = defaultUser,
                        Account = tmpAccount,
                        Franchise = lead.Franchise
                    };

                    var body = RenderViewToString("Emails/LeadSecond", model);
                    Email.SendMarketingEmail(model.To, model.From, "Let Moovers Move you", body, EmailCategory.SecondLeadContact, lead.LeadID);
                    lead.EmailsSent++;
                    leadRepo.Save();

                    second.Add(lead.Email);
                }

                return Json(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + String.Join(",", first) + ":" + String.Join(",", second), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Storage invoices are generated several several days before they are due.
        /// 
        /// This generates an invoice and e-mails the invoice to customers who have online invoicing.
        /// </summary>
        /// <returns></returns>
        public ActionResult InvoiceStorage()
        {
            lock (storageLock)
            {
                // create invoices
                var items = storageRepo.GetForInvoice(DateTime.Today);
                foreach (var item in items)
                {
                    item.CreateInvoice();
                }

                storageRepo.Save();

                // send paperless invoices to customers enrolled in them
                var forEmail = storageRepo.GetForPaperlessInvoice().ToList().Where(s => s.GetActiveInvoices().Any(i => !i.IsPrinted && i.GetBalance() > 0));
                foreach (var workOrder in forEmail)
                {
                    var invoices = workOrder.GetActiveInvoices().Where(i => !i.IsPrinted);
                    foreach (var i in invoices)
                    {
                        i.IsPrinted = true;
                        storageRepo.Save();
                    }

                    var forDate = DateTime.Now;
                    var model = new StorageStatementModel() {
                        Date = forDate,
                        Franchise = franchiseRepo.GetStorage(),
                        StorageWorkOrders = new [] { workOrder }
                    };

                    var fileContent = RenderViewToString("PDFS/_StorageStatement", model);
                    var pdf = Business.Utility.General.GeneratePdf(fileContent, System.Drawing.Printing.PaperKind.Letter);

                    // save a record of the statement in the storage account, logging the userid as "auto"
                    var statement = workOrder.AddStatement(null, fileContent);
                    storageRepo.Save();

                    var file = statement.File;
                    file.SavedContent = pdf;
                    storageRepo.Save();

                    var email = workOrder.Account.GetEmail(Business.Models.EmailAddressType.Primary);
                    if (email != null)
                    {
                        var emailContent = RenderViewToString("Emails/StorageInvoice", workOrder);
                        Email.SendLoggedEmail(storageRepo, workOrder, email.Email, "support@1800moovers.com", Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                            "Your Moovers Storage", emailContent, new [] { file }, EmailCategory.StorageInvoice);
                    }
                }

                return Json(String.Join(", ", items.Select(i => i.Lookup)), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// For storage accounts on automatic billing, this charges their credit card for the latest invoice.
        /// </summary>
        /// <returns></returns>
        public ActionResult ChargeStorage()
        {
            lock (storageLock)
            {
                var forAutoBill = storageRepo.GetForAutomaticBilling().ToList();
                var franchise = franchiseRepo.GetStorage();
                var due = forAutoBill.Where(i => i.GetOverdueBalance() > 0).Take(1);
                var charged = new Dictionary<string, decimal>();

                foreach (var workOrder in due)
                {
                    string transactionID = new PaymentRepository().GetUniqueTransactionID();
                    decimal amount = workOrder.GetOverdueBalance();
                    var card = workOrder.Account_CreditCard;
                    string errors;

                    string email = String.Empty;
                    var emailAcct = workOrder.Account.GetEmail(Business.Models.EmailAddressType.Primary);
                    if (emailAcct != null)
                    {
                        email = emailAcct.Email;
                    }


                    var cardPayment = new CreditCardPayment();
                    bool successful = false;

                    var payment = cardPayment.ChargeCreditCard(
                        franchise: franchise,
                        payable: workOrder,
                        creditCardID: card.CreditCardID,
                        transArmorToken: card.GetCard(),
                        cardholder: card.GetCardHolder(),
                        cardType: card.CardType,
                        expiry: card.GetExpiration(),
                        amount: amount,
                        customerid: workOrder.Account.Lookup,
                        transactionid: transactionID,
                        clientEmail: email,
                        errorMessage: out errors,
                        successful: out successful,
                        paymentRepo: storageRepo
                    );

                    storageRepo.Save();

                    if (!successful)
                    {
                        workOrder.LastAutomaticBillingDate = DateTime.Now;
                        workOrder.FailedAutomaticBillingAttempts++;
                    }

                    storageRepo.Save();

                    // attempt to bill credit cards 4x before we send them a notice that it failed
                    if (!successful && workOrder.FailedAutomaticBillingAttempts >= 4)
                    {
                        workOrder.HasPaymentError = true;
                        workOrder.LastPaymentError = errors;

                        if (!String.IsNullOrEmpty(email))
                        {
                            var invoice = workOrder.GetActiveInvoices().Where(i => !i.IsPaid()).OrderBy(i => i.ForDate).First();
                            var emailModel = new StoragePaymentEmail() {
                                Invoice = invoice,
                                Payment = payment,
                                WorkOrder = workOrder
                            };

                            var storageStatementModel = new StorageStatementModel() {
                                Date = invoice.ForDate,
                                Franchise = franchiseRepo.GetStorage(),
                                StorageWorkOrders = new[] {
                                    workOrder
                                }
                            };
                            var fileContent = RenderViewToString("PDFS/_StorageStatement", storageStatementModel);
                            var pdf = General.GeneratePdf(fileContent, System.Drawing.Printing.PaperKind.Letter);

                            var file = new Business.Models.File("Storage Statement - " + DateTime.Now.ToShortDateString(), "application/pdf");
                            fileRepo.Add(file);
                            fileRepo.Save();

                            file.SavedContent = pdf;
                            fileRepo.Save();

                            var emailBody = RenderViewToString("Emails/_StoragePaymentDeclined", emailModel);
                            Email.SendLoggedEmail(
                                repo: storageRepo,
                                obj: workOrder,
                                to: email,
                                from: "alert@1800moovers.com",
                                ccs: Enumerable.Empty<string>(),
                                bccs: Enumerable.Empty<string>(),
                                subject: "Account Alert: AutoPay Declined",
                                body: emailBody,
                                files: new[] { file },
                                category: EmailCategory.StorageDeclined
                            );
                        }
                    }

                    if (successful)
                    {
                        workOrder.ResetAutomaticBilling();
                    }

                    charged.Add(workOrder.Lookup, amount);
                    storageRepo.Save();
                }

                return Json(String.Join(",", charged.Select(i => i.Key + ":" + i.Value.ToString())), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// For Scheduled moves, this e-mails customers requesting them to confirm their inventory, addresses, and move date.
        /// </summary>
        /// <returns></returns>
        public ActionResult SendMoveConfirmEmails()
        {
            lock (quoteLock)
            {
                var in7days = quoteRepo.GetBookedBetween(null, DateTime.Today.AddDays(1), DateTime.Today.AddDays(7).AddMilliseconds(-1)).Where(i => i.ConfirmationEmailsSent == 0 && !i.Schedules.Any(s => s.IsConfirmed)).ToList();
                in7days = in7days.Where(i => i.DateScheduled.HasValue && i.DateScheduled.Value.Date != DateTime.Today).ToList();

                var ret = new List<string>();
                foreach (var quote in in7days.Take(1))
                {
                    var email = quote.Account.GetEmail(Business.Models.EmailAddressType.Primary);
                    if (email == null)
                    {
                        quote.ConfirmationEmailsSent++;
                        quoteRepo.Save();
                        return Json("Skipped " + quote.Lookup, JsonRequestBehavior.AllowGet);
                    }

                    var file = GetQuoteFile(quote.QuoteID, "-New Proposal-");
                    var confirm = confirmRepo.CreateNew();
                    confirm.QuoteID = quote.QuoteID;
                    confirmRepo.Add(confirm);
                    confirmRepo.Save();

                    quote.ConfirmationEmailsSent++;
                    quoteRepo.Save();

                    string body = RenderViewToString("Emails/MoveConfirmation", confirm);
                    Email.SendLoggedEmail(quoteRepo, quote, email.Email, quote.AccountManager.aspnet_Membership.Email, Enumerable.Empty<string>(), Enumerable.Empty<string>(),
                        "Confirm Your Move on " + quote.GetSchedules().Min(i => i.Date).ToShortDateString(),
                        body,
                        new [] { file },
                        EmailCategory.Confirmation
                    );

                    ret.Add(quote.Lookup);
                }

                return Json(String.Join(",", ret), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Add files from email
        /// </summary>
        /// <returns></returns>
        public ActionResult ParseFiles()
        {
            Business.Utility.FileParser.GetFiles();
            return Content(String.Empty);
        }
        /// <summary>
        /// Contracts are often generated in large lists, and to speed this up, we store html content in the DB. This function generates PDFs from the stored html content and removes
        /// the html (helps minimize DB size)
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        public ActionResult MinimizeContractFiles(int take = 10)
        {
            var files = fileRepo.db.Files.Where(i => i.HtmlContent != null).Take(take).ToList();
            foreach (var f in files)
            {
                f.SavedContent =  General.GeneratePdf(f.HtmlContent, System.Drawing.Printing.PaperKind.Legal);
                f.HtmlContent = null;
                fileRepo.Save();
            }


            return Json(String.Join(",", files.Select(i => i.FileID)), JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailTodaysLeads()
        {
            var leads = leadRepo.GetTodayLeads();
            var franchises = leads.ToList().GroupBy(l => l.Franchise.Name);

            StringBuilder sb = new StringBuilder();
            franchises.ToList().ForEach(f =>
            {
                sb.Clear();
               var sources =  f.GroupBy(s => s.Source).Select(s => new { Franchise = f.Key, Name = s.Key, Count = s.Count() }).ToList();
               sources.ForEach(s =>
               {
                   sb.Append("<br>");
                   sb.Append(s.Count + (s.Count>1 ?" Leads" : " Lead") + " received today from " + s.Name);
              
               });
                
               //// Get the store manager email and send email
               //var email = f.FirstOrDefault().Franchise.Franchise_aspnetUser.Where(u => u.IsManager).FirstOrDefault().aspnet_Users.aspnet_Membership.Email;
               //if (!String.IsNullOrEmpty(email))
               //{
               //    Email.SendEmail(email, "support@1800moovers.com", "Leads Status", String.Format("{0} have " + sb, f.Key));
               //}
            });
            return Json(string.Join(",",leads.Select(l=>l.Source)), JsonRequestBehavior.AllowGet);
        }
    }
}
