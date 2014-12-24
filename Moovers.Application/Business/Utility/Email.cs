using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Business.Repository.Models;

namespace Business.Utility
{
    using System.Web;

    using Business.Models;

    using File = System.IO.File;

    public enum EmailCategory
    {
        [Description("")]
        None,
        
        
        [Description("Storage Declined")]
        StorageDeclined,

        [Description("Proposal")]
        Proposal,

        [Description("Unbooked Quote")]
        UnbookedQuote,

        [Description("Storage Invoice")]
        StorageInvoice,

        [Description("Confirmation")]
        Confirmation,

        [Description("Initial Lead Contact")]
        InitialLeadContact,

        [Description("Second Lead Contact")]
        SecondLeadContact,

        [Description("Webquote Received")]
        WebQuoteReceived


    }

    public static class Email
    {
        private static bool IsDevelopment
        {
            get
            {
    #if RELEASE
                return false;
    #else
                return true;
    #endif
            }
        }

        public static string PlainTextToHtml(string plaintext)
        {
            return plaintext.Replace(System.Environment.NewLine, "<br />");
        }

        /// <summary>
        /// All emails in development are sent to this address instead of the "TO" address
        /// </summary>
        public static string DevelopmentEmail
        {
            get { return "zubair1599@gmail.com"; }
        }

        private static string EmailUrl
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["MailServer"]; }
        }

        private static string EmailUser
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["MailUsername"]; }
        }

        private static string EmailPassword
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["MailPassword"]; }
        }

        private static int EmailPort
        {
            get { return int.Parse(System.Configuration.ConfigurationManager.AppSettings["MailPort"]); }
        }

        public static void SendLoggedEmail<T>(Models.RepositoryBase<T> repo, T obj, ViewModels.EmailModel emailModel, string subject, string body, EmailCategory category = EmailCategory.None, bool isMarketing = false)
            where T: class
        {
            SendLoggedEmail(
                repo: repo,
                obj: obj,
                to: emailModel.To,
                from: emailModel.From,
                ccs: Enumerable.Empty<string>(),
                bccs: Enumerable.Empty<string>(),
                subject: subject,
                body: body,
                files: Enumerable.Empty<Models.File>(),
                category: category,
                isMarketing: isMarketing
            );
        }

        /// <summary>
        /// Certain types of e-mails are extensively logged and monitored.*/
        /// </summary>
        /// <typeparam name="T">Quote or StorageWorkOrder</typeparam>
        public static void SendLoggedEmail<T>(Models.RepositoryBase<T> repo, T obj, string to, string from, IEnumerable<string> ccs, IEnumerable<string> bccs, string subject, string body, IEnumerable<Models.File> files, EmailCategory category, bool isMarketing = false)
            where T: class
        {
            /*** TODO: currently only quote/storage are logged in this way. If there are more, should probably look into cleaning this up a little */
            if (typeof(T) != typeof(Models.Quote) && typeof(T) != typeof(Models.StorageWorkOrder))
            {
                throw new InvalidOperationException("Currently only Quotes and Storage have logged e-mails implemented");
            }

            var emailLog = new Models.EmailLog();
            var logrepo = new EmailLogRepository();
            logrepo.SetBase(repo);
            logrepo.Add(emailLog);

            

            var attachments = new List<System.Net.Mail.Attachment>();
            files = files ?? Enumerable.Empty<Models.File>();

            if (files.Any(x => x.Name.Equals("CustomerResponsibilityChecklist.pdf")))
            {
                var file = files.First(x => x.Name.Equals("CustomerResponsibilityChecklist.pdf"));
                var path = HttpContext.Current.Server.MapPath("~/" + file.Name);
                var chklstmemstream = new System.IO.MemoryStream(File.ReadAllBytes(path));
                var chklstattachment = new System.Net.Mail.Attachment(chklstmemstream, file.Name, file.ContentType);
                attachments.Add(chklstattachment);
            }

            if (files.Any(x => x.Name.Equals("YourRightsandProtectionofYourProperty.pdf")))
            {
                var file = files.First(x => x.Name.Equals("YourRightsandProtectionofYourProperty.pdf"));
                var path = HttpContext.Current.Server.MapPath("~/" + file.Name);
                var chklstmemstream = new System.IO.MemoryStream(File.ReadAllBytes(path));
                var chklstattachment = new System.Net.Mail.Attachment(chklstmemstream, file.Name, file.ContentType);
                attachments.Add(chklstattachment);
            }

            foreach (var file in files.Where(x => !x.Name.Equals("CustomerResponsibilityChecklist.pdf") && !x.Name.Equals("YourRightsandProtectionofYourProperty.pdf")).ToList())
            {
                var memstream = new System.IO.MemoryStream(file.SavedContent);
                var attachment = new System.Net.Mail.Attachment(memstream, file.Name, file.ContentType);
                attachments.Add(attachment);
                emailLog.AddFile(file.FileID);
            }

            emailLog.MailTo = to;
            emailLog.MailFrom = from;
            emailLog.Subject = subject;
            emailLog.Message = body;
            emailLog.DateSent = DateTime.Now;
            emailLog.Quote = obj as Models.Quote;
            emailLog.StorageWorkOrder = obj as Models.StorageWorkOrder;

            logrepo.Save();

            if (isMarketing)
            {
                SendMarketingEmail(to, from, subject, body, category);
            }
            else
            {
                SendEmail(to, from, ccs, bccs, subject, body, attachments, category);
            }
        }

        /// <summary>
        /// Marketing emails have special rules -- they are logged differently, and they must first check for "unsubscribed" users before e-mailing.
        /// </summary>
        public static void SendMarketingEmail(string to, string from, string subject, string body, EmailCategory category = EmailCategory.None, Guid? leadID = null)
        {
            var repo = new EmailUnsubscribeRepository();
            var logRepo = new MarketingEmailLogRepository();

            var log = new Models.MarketingEmailLog() {
                MailTo = to,
                MailFrom = from,
                Subject = subject,
                Content = body,
                DateSent = DateTime.Now,
                LeadID = leadID,
                WasUnsubscribed = false
            };

            logRepo.Add(log);

            // we can't e-mail users on the no-mailing list with marketing e-mails, instead just log the request and move on.
            if (repo.Exists(to))
            {
                log.WasUnsubscribed = true;
            }
            else
            {
                SendEmail(to, from, Enumerable.Empty<string>(), Enumerable.Empty<string>(), subject, body, Enumerable.Empty<Attachment>(), category);
            }

            logRepo.Save();
        }

        public static void SendEmail(string to, string from, string subject, string body, EmailCategory category = EmailCategory.None)
        {
            SendEmail(to, from, Enumerable.Empty<string>(), Enumerable.Empty<string>(), subject, body, Enumerable.Empty<Attachment>(), category);
        }

        private static void SendEmail(string to, string from, IEnumerable<string> cc, IEnumerable<string> bcc, string subject, string body, IEnumerable<Attachment> attachments, EmailCategory category = EmailCategory.None)
        {
            var client = new SmtpClient(EmailUrl);
            var credentials = new NetworkCredential(EmailUser, EmailPassword);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;
            client.EnableSsl = true;
            client.Port = EmailPort;

            // in development, always e-mail the developer instead of sending e-mails to actual customers
            if (IsDevelopment)
            {
                subject += "Originally Intended for: " + to;
                to = DevelopmentEmail;
            }

            var message = new MailMessage(from,to,subject,body) {
                IsBodyHtml = true,
            };

            dynamic customHeaders = new System.Dynamic.ExpandoObject();
            if (category != EmailCategory.None)
            {
                customHeaders.category = category.GetDescription();
            }

            foreach (var addr in cc)
            {
                message.CC.Add(new MailAddress(addr));
            }

            foreach (var addr in bcc)
            {
                message.Bcc.Add(new MailAddress(addr));
            }

            foreach (var item in attachments)
            {
                message.Attachments.Add(item);
            }

            // sendgrid custom header
            // http://sendgrid.com/docs/API_Reference/SMTP_API/
            message.Headers.Add("X-SMTPAPI", Utility.LocalExtensions.SerializeToJson(customHeaders));

            try
            {
                client.Send(message);
            }
            catch (Exception e)
            {
                var repo = new ErrorRepository();
                repo.Log(e, "Email Error", new System.Collections.Specialized.NameValueCollection(), new System.Collections.Specialized.NameValueCollection());
                repo.Save();
            }
        }
    }
}
