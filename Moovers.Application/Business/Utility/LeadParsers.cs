using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utility
{
    public class LocalMoversLeadParser : LeadParser
    {
        public override string Name
        {
            get { return "Local Movers"; }
        }

        public override bool CanParse(AE.Net.Mail.MailMessage message)
        {
            if (message.From.Address.Contains("move@123movers.com"))
            {
                return true;
            }

            if (message.Subject.Contains("Local Movers Lead"))
            {
                return true;
            }

            return false;
        }

        public override JsonObjects.LeadJson Parse(string leadText)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(leadText);

            var nodes = new List<string>();
            foreach (var row in htmlDoc.DocumentNode.SelectNodes("//tr").Skip(1))
            {
                nodes.Add(row.LastChild.InnerText);
            }

            var ret = new JsonObjects.LeadJson {
                ID = nodes[0],
                Name = nodes[1],
                CurrentZip = nodes[2],
                HomePhone = nodes[3],
                WorkPhone = nodes[4],
                Email = nodes[5],
                ContactPreference = nodes[6]
            };

            try
            {
                ret.MoveDate = DateTime.Parse(nodes[7]);
            }
            catch (System.FormatException)
            {
                ret.MoveDate = null;
            }

            ret.Weight = nodes[8];
            ret.Origin = nodes[9];
            ret.Destination = nodes[10];
            ret.Comments = nodes[11];

            try
            {
                ret.DateSubmitted = DateTime.Parse(nodes[12]);
            }
            catch (System.FormatException)
            {
                ret.DateSubmitted = null;
            }

            return ret;
        }
    }

    /// <summary>
    /// Moving.com lead parse -- these leads come in plain text emails, with different fields seaprated by new lines.
    /// 
    /// Example:
    /// Contact Information
    ///     * First Name: Ted
    ///     * Last Name: Wallinger
    ///    (etc)
    /// </summary>
    public class MovingComLeadParser : LeadParser
    {
        public override string Name
        {
            get { return "Moving.com"; }
        }

        public override bool CanParse(AE.Net.Mail.MailMessage message)
        {
            var moovingSubjectLines = new string[] {
                "Lead for Moovers, Inc.",
                "Lead for Moovers of St Louis Inc."
            }.Select(i => i.ToLower());

            return moovingSubjectLines.Any(m => message.Subject.ToLower().Contains(m));
        }

        public override JsonObjects.LeadJson Parse(string leadText)
        {
            IEnumerable<string> lines = leadText.Split('\n');
            
            var ret = new JsonObjects.LeadJson();

            // all lines we care about are formatted in "Title":"value" format 
            lines = lines.Where(l => l.Contains(":"));

            // format each line into a key/value pair for easier management
            var kvps = lines.Select(l => new KeyValuePair<string, string>(
                l.Substring(0, l.IndexOf(":")).Replace("*", String.Empty).Trim(),
                l.Substring(l.IndexOf(":") + 1).Trim()
            ));

            foreach (var kvp in kvps)
            {
                if (kvp.Key == "ID")
                {
                    ret.ID = kvp.Value;
                }

                // Many leads we receive do not have first/last names. Just combine Moving.com first/last name into a single field
                if (kvp.Key == "First Name")
                {
                    ret.Name = (ret.Name + " " + kvp.Value).Trim();
                }

                if (kvp.Key == "Last Name")
                {
                    ret.Name = (ret.Name + " " + kvp.Value).Trim();
                }

                if (kvp.Key == "Work Phone")
                {
                    ret.WorkPhone = kvp.Value;
                }

                if (kvp.Key == "From Zip")
                {
                    ret.CurrentZip = kvp.Value;
                }

                if (kvp.Key == "From City")
                {
                    ret.CurrentCity = kvp.Value;
                }

                if (kvp.Key == "From State")
                {
                    ret.CurrentState = kvp.Value;
                }

                if (kvp.Key == "Home Phone")
                {
                    ret.HomePhone = kvp.Value;
                }

                if (kvp.Key == "Mobile Phone")
                {
                    ret.MobilePhone = kvp.Value;
                }

                if (kvp.Key == "Email")
                {
                    ret.Email = kvp.Value;
                }

                if (kvp.Key == "Best Time To Call")
                {
                    ret.ContactPreference = kvp.Value;
                }

                if (kvp.Key == "Can We Call You At Work")
                {
                }

                if (kvp.Key == "To State")
                {
                    ret.Destination = (ret.Destination + " " + kvp.Value).Trim();
                }

                if (kvp.Key == "To City")
                {
                    ret.Destination = (kvp.Value + ", " + ret.Destination).Trim();
                }

                if (kvp.Key == "Approx Move Date")
                {
                    ret.MoveDate = DateTime.Parse(kvp.Value);
                }

                if (kvp.Key == "Approx Move Weight")
                {
                    ret.Weight = kvp.Value;
                }

                if (kvp.Key == "Additional Comments")
                {
                    ret.Comments = kvp.Value;
                }
            }

            ret.Origin = ret.CurrentCity + ", " + ret.CurrentState + " " + ret.CurrentZip;

            return ret;
        }
    }
}
