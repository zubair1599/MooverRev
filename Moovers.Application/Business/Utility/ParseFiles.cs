using AE.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Business.Enums;
using Business.Repository;
using Business.Repository.Models;

namespace Business.Utility
{
    public static class FileParser
    {
        private static string Server = System.Configuration.ConfigurationManager.AppSettings["FileServer"];
        private static string Username = System.Configuration.ConfigurationManager.AppSettings["FileUsername"];
        private static string Password = System.Configuration.ConfigurationManager.AppSettings["FilePassword"];
        private static int Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["FilePort"]);

        private static Dictionary<string, Regex> SubjectLines = new Dictionary<string, Regex>() {
            { "Employees", new Regex("employee", RegexOptions.IgnoreCase) }
        };

        public class EmployeeNumberParser
        {
            public string Parse(string subject)
            {
                var regex = new Regex(@"(?!\w)#([0-9]+)(?!\w)");
                
                if (regex.IsMatch(subject))
                {
                    return regex.Matches(subject)[0].Groups[1].Value;
                }

                return String.Empty;
            }
        }

        public class EmployeeFileTypeParser
        {
            public Employee_File_Type Parse(string subject)
            {
                if (subject.Contains("dot"))
                {
                    return Employee_File_Type.DOTCard;
                }

                return Employee_File_Type.DriverLicense;
            }
        }

        public static IEnumerable<Models.Employee_File_Rel> GetFiles()
        {
            var added = new List<Models.Employee_File_Rel>();
            using (var client = new ImapClient(Server, Username, Password, AE.Net.Mail.ImapClient.AuthMethods.Login, Port, true))
            {
                var msgs = client.SearchMessages(SearchCondition.Unseen(), false, true);
                var repo = new LeadRepository();
                var parsed = new List<MailMessage>();

                int max = 10;
                int count = 0;
                foreach (var msg in msgs)
                {
                    count++;
                    if (count > max)
                    {
                        break;
                    }

                    var fetchedMsg = msg.Value;
                    client.AddFlags(Flags.Seen, fetchedMsg);
                    foreach (var subjectLine in SubjectLines)
                    {
                        if (subjectLine.Value.IsMatch(fetchedMsg.Subject))
                        {
                            var msgText = fetchedMsg.Body;
                            if (String.IsNullOrEmpty(msgText))
                            {
                                continue;
                            }

                            var employeeNumber = new EmployeeNumberParser().Parse(fetchedMsg.Subject);
                            var type = new EmployeeFileTypeParser().Parse(fetchedMsg.Subject);

                            var attachment = fetchedMsg.Attachments.FirstOrDefault();
                            if (!String.IsNullOrEmpty(employeeNumber) && attachment != null)
                            {
                                var employeeRepo = new EmployeeRepository();
                                var emp = employeeRepo.Get(employeeNumber);
                                if (emp != null)
                                {
                                    added.Add(emp.AddFile(type, attachment.Filename, attachment.GetData(), attachment.ContentType));
                                    employeeRepo.Save();
                                }
                            }

                            parsed.Add(fetchedMsg);
                            break;
                        }
                    }
                }

                repo.Save();
                return added;
            }
        }
    }
}
