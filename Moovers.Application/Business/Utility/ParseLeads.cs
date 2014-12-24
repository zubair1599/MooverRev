using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AE.Net.Mail;
using Business.Repository.Models;
using Business.Utility;

namespace Business.Utility
{
    public abstract class LeadParser
    {      
        public abstract string Name { get; }

        public abstract JsonObjects.LeadJson Parse(string leadText);

        public abstract bool CanParse(MailMessage message);
    }

    public class LeadManager
    {        
        private readonly Interfaces.IFranchiseRepository franchiseRepo;

        private readonly Interfaces.IZipcodeRepository zipRepo;

        public LeadManager()
        {
            this.franchiseRepo = new FranchiseRepository();
            this.zipRepo = new ZipCodeRepository();
        }

        private LeadParser GetParser(MailMessage message)
        {
            var parsers = new LeadParser[] {
                new LocalMoversLeadParser(),
                new MovingComLeadParser()
            };

            foreach (var p in parsers)
            {
                if (p.CanParse(message))
                {
                    return p;
                }
            }

            return null;
        }

        public IEnumerable<Models.Lead> GetLeads(IRemoteLeadRepository source)
        {
            var added = new List<Models.Lead>();
            foreach (var message in source.GetMessages())
            {
                var parser = this.GetParser(message);
                if (String.IsNullOrEmpty(message.Body) || parser == null)
                {
                    continue;
                }

                var leadObj = parser.Parse(message.Body);
                var leadJson = leadObj.SerializeToJson();
                var zipcode = zipRepo.Get(leadObj.CurrentZip) ?? zipRepo.GetByFirst3(leadObj.CurrentZip.Substring(0, 3));
                var franchise = franchiseRepo.GetClosestTo(zipcode);
                var lead = new Models.Lead() {
                    LeadJson = leadJson,
                    MessageText = message.Body,
                    Source = parser.Name,
                    AddedDate = DateTime.Now,
                    Name = leadObj.Name,
                    Email = leadObj.Email,
                    FranchiseID = franchise.FranchiseID
                };

                added.Add(lead);
                source.MarkParsed(message);
            }

            return added;
        }
    }
}