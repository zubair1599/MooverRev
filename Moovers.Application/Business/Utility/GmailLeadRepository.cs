using AE.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utility
{
    public interface IRemoteLeadRepository
    {
        IEnumerable<MailMessage> GetMessages();

        void MarkParsed(MailMessage message);
    }

    public class GmailLeadRepository : IRemoteLeadRepository, IDisposable
    {
        private static readonly string LeadServer = System.Configuration.ConfigurationManager.AppSettings["LeadServer"];
        private static readonly string LeadUsername = System.Configuration.ConfigurationManager.AppSettings["LeadUsername"];
        private static readonly string LeadPassword = System.Configuration.ConfigurationManager.AppSettings["LeadPassword"];
        private static readonly int LeadPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["LeadPort"]);

        private readonly ImapClient client;

        public GmailLeadRepository()
        {
            this.client = new ImapClient(LeadServer, LeadUsername, LeadPassword, AE.Net.Mail.ImapClient.AuthMethods.Login, LeadPort, true);
        }

        public IEnumerable<MailMessage> GetMessages()
        {
            int max = 10;
            var msgs = this.client.SearchMessages(SearchCondition.Unseen(), false, true).Take(max);
            return msgs.Select(i => i.Value);
        }

        public void MarkParsed(MailMessage message)
        {
            this.client.AddFlags(Flags.Seen, message);
        }

        public void Dispose()
        {
            this.client.Dispose();
        }
    }

}
