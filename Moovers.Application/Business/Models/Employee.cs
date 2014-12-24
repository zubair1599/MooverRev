// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Employee.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.Repository.Models;
    using Business.Utility;

    public partial class Employee
    {
        public FilingStatus FilingStatus
        {
            get
            {
                if (!FilingStatusID.HasValue)
                {
                    return FilingStatus.Single;
                }

                return (FilingStatus)FilingStatusID;
            }

            set { FilingStatusID = (int)value; }
        }

        public PayType PayType
        {
            get
            {
                if (!PayTypeID.HasValue)
                {
                    return PayType.Hourly;
                }

                return (PayType)PayTypeID;
            }

            set { PayTypeID = (int)value; }
        }

        public Gender Gender
        {
            get
            {
                if (!GenderID.HasValue)
                {
                    return Gender.Male;
                }

                return (Gender)GenderID;
            }

            set { GenderID = (int)value; }
        }

        public string SocialSecurityNumber
        {
            get
            {
                if (!String.IsNullOrEmpty(EncryptedSSN))
                {
                    return Security.Decrypt(EncryptedSSN);
                }

                return null;
            }

            set { EncryptedSSN = Security.Encrypt(value ?? String.Empty); }
        }

        public Position PositionType
        {
            get { return (Position)PositionTypeID; }

            set { PositionTypeID = (int)value; }
        }

        public EmployeeStatus EmployeeStatus
        {
            get { return (EmployeeStatus)StatusId; }

            set { StatusId = (int)value; }
        }

        public EmailAddress Email
        {
            get
            {
                if (String.IsNullOrEmpty(EmailString))
                {
                    return null;
                }

                var ret = new EmailAddress { Email = EmailString };
                return ret;
            }

            set { EmailString = (value == null) ? String.Empty : value.Email; }
        }

        public PhoneNumber PrimaryPhone
        {
            get
            {
                if (String.IsNullOrEmpty(PrimaryPhoneString))
                {
                    return null;
                }

                var ret = new PhoneNumber(PrimaryPhoneString) { Extension = PrimaryPhoneExtension };
                return ret;
            }

            set
            {
                if (value == null)
                {
                    PrimaryPhoneString = null;
                    PrimaryPhoneExtension = null;
                }
                else
                {
                    PrimaryPhoneString = value.Number;
                    PrimaryPhoneExtension = value.Extension;
                }
            }
        }

        public PhoneNumber SecondaryPhone
        {
            get
            {
                if (String.IsNullOrEmpty(SecondaryPhoneString))
                {
                    return null;
                }

                var ret = new PhoneNumber(SecondaryPhoneString) { Extension = SecondaryPhoneExtensionString };
                return ret;
            }

            set
            {
                if (value == null)
                {
                    SecondaryPhoneString = null;
                    SecondaryPhoneExtensionString = null;
                }
                else
                {
                    SecondaryPhoneString = value.Number;
                    SecondaryPhoneExtensionString = value.Extension;
                }
            }
        }

        public object ToJsonObject()
        {
            return
                new
                {
                    EmployeeID = EmployeeID,
                    Lookup = Lookup,
                    NameFirst = NameFirst,
                    NameLast = NameLast,
                    Wage = Wage,
                    Email = (Email != null) ? Email.Email : String.Empty,
                    PrimaryPhone = (PrimaryPhone != null) ? PrimaryPhone.Number : String.Empty,
                    SecondaryPhone = (SecondaryPhone != null) ? SecondaryPhone.Number : String.Empty,
                    FranchiseID = FranchiseID
                };
        }

        public string DisplayName()
        {
            return NameFirst + " " + NameLast;
        }

        public string DisplayNumber()
        {
            if (PrimaryPhone == null)
            {
                return "";
            }

            return PrimaryPhone.DisplayString();
        }

        public string DisplayShortName()
        {
            return NameFirst.Substring(0, 1) + ". " + NameLast;
        }

        public Guid GetAspUserID()
        {
            if (Employee_aspnet_User_Rel.Any())
            {
                return Employee_aspnet_User_Rel.First().aspnet_UserID;
            }

            return default(Guid);
        }

        public decimal GetManHourRateBetween(DateTime startDate, DateTime endDate, IEnumerable<Posting_Employee_Rel> jobs = null)
        {
            if (jobs == null)
            {
                jobs = GetJobs(startDate, endDate);
            }

            List<Quote> quotes =
                jobs.Where(i => !i.Posting.Schedule.IsCancelled)
                    .Select(i => i.Posting.Quote)
                    .Where(i => i.PricingTypeID == (int)QuotePricingType.Binding)
                    .Where(i => !i.Postings.Any(p => p.Posting_Employee_Rel.Any(r => r.Commission > 0)))
                    .Distinct()
                    .ToList();

            if (!quotes.Any())
            {
                return 0;
            }

            decimal hours = quotes.Sum(i => i.Postings.Sum(p => p.GetManHours()));
            decimal dollars = quotes.Sum(i => i.GuaranteeData.GuaranteedPrice);

            return (dollars / hours);
        }

        public decimal GetManHoursBetween(DateTime startDate, DateTime endDate, IEnumerable<Posting_Employee_Rel> jobs = null)
        {
            if (jobs == null)
            {
                jobs = GetJobs(startDate, endDate);
            }

            if (!jobs.Any())
            {
                return 0;
            }

            decimal hours = jobs.Sum(i => i.Hours);

            return Math.Round(hours);
        }

        public IEnumerable<Posting_Employee_Rel> GetJobs(DateTime start, DateTime end)
        {
            var repo = new EmployeeRepository();
            return repo.GetJobs(EmployeeID, start, end);
        }

        public int GetJobCount()
        {
            var repo = new EmployeeRepository();
            return PreviousMoveCount + repo.GetJobs(EmployeeID).Count();
        }

        public IQueryable<Posting_Employee_Rel> GetLatestJobs(int count = 10)
        {
            var repo = new EmployeeRepository();
            return repo.GetJobs(EmployeeID).OrderByDescending(i => i.Posting.Schedule.Date).Take(count);
        }

        public Employee_File_Rel GetFile(Employee_File_Type type)
        {
            return Employee_File_Rel.FirstOrDefault(i => i.Type == (int)type);
        }

        public Employee_File_Rel AddFile(Employee_File_Type type, string name, byte[] content, string contentType)
        {
            var repo = new FileRepository();
            var file = new File(name, contentType);
            repo.Add(file);
            repo.Save();

            file.SavedContent = content;
            repo.Save();

            Employee_File_Rel rel = GetFile(type);
            if (rel == null)
            {
                rel = new Employee_File_Rel();
                Employee_File_Rel.Add(rel);
            }

            rel.FileID = file.FileID;
            rel.EmployeeID = EmployeeID;
            rel.Type = (int)type;

            return rel;
        }
    }
}