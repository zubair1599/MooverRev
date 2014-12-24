using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Business.Enums;
using Business.Repository;
using Business.Repository.Models;
using Business.ToClean;
using Business.ToClean.QuoteHelpers;
using Business.Utility;
using Moovers.WebModels;

namespace MooversCRM.Controllers
{
    public class NonSecureController : BaseControllers.NonSecureBaseController
    {
        private static StopAddressType GetAddressType(string remoteType)
        {
            var ret = StopAddressType.House;

            if (remoteType == "House 2-Story")
            {
                ret = StopAddressType.House;
            }

            if (remoteType == "House Multilevel")
            {
                ret = StopAddressType.House;
            }

            if (remoteType == "Townhouse/Condo 2-Story")
            {
                ret = StopAddressType.Multiplex;
            }

            if (remoteType == "Townhouse/Condo 1-Story")
            {
                ret = StopAddressType.Multiplex;
            }

            if (remoteType == "Townhouse/Condo Multilevel")
            {
                ret = StopAddressType.Multiplex;
            }

            if (remoteType == "Apartment 1st Floor")
            {
                ret = StopAddressType.Apartment;
            }

            if (remoteType == "Apartment 2nd Floor")
            {
                ret = StopAddressType.Apartment;
            }

            if (remoteType == "Apartment 3rd Floor")
            {
                ret = StopAddressType.Apartment;
            }

            return ret;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AddExternalQuote(
            /*string QUOTEID, *//*string EST_MOVE_DATE,*/ string EST_MOVE_DAY, string EST_MOVE_DATE_YEAR, string EST_MOVE_DATE_MONTH, string CUST_FIRST_NAME, string CUST_LAST_NAME,
            string CUST_PRIMARY_PHONE, string CUST_MOBILE_PHONE, string CUST_FAX_PHONE, string CUST_PRIMARY_EMAIL, /*string CUST_COMPANY_NAME, string CUST_BID_TYPE,*/
            string STOP1_ZIP, string STOP1_ACTBUILDING_TYPE, string STOP4_ZIP, string STOP4_ACTBUILDING_TYPE, /*string LEAD_ID, string advSource,*/
            string cust_notes, string InventoryMeta, string InventoryData, string CustomInventoryMeta, string CustomInventoryData
                ////string STOP4_WALKS, //string STOP4_BUILDING_TYPE, //string CUST_CONTACT_PREF, //string EST_MOVE_DATE_DAY, //string MODY, // unused //string MODM, // unused //string MODD,// unused //string STOP2_ZIP, //string STOP2_STAIRS, //string STOP2_WALKS, //string STOP3_ZIP, //string STOP3_STAIRS, //string STOP3_WALKS, //string STOP4_ADDRESS, //string STOP4_CITY, //string STOP4_STATE, //string STOP4_APTSUITE, //string STOP1_STAIRS, //string STOP1_WALKS, //string STOP1_BUILDING_TYPE, //string CUST_MOVE_DETAIL, //string CUST_AD_SOURCE, //string CUST_INTERNAL_NOTES, //string STOP1_ADDRESS, //string STOP1_CITY, //string STOP1_STATE, //string CUST_MOVE_TYPE, //string STOP1_APTSUITE 
            )
        {
            STOP1_ZIP = (STOP1_ZIP ?? String.Empty).Trim();
            STOP4_ZIP = (STOP4_ZIP ?? String.Empty).Trim();

            var accountRepo = new AccountRepository();

            var account = new Business.Models.PersonAccount {
                FranchiseID = new FranchiseRepository().GetStorage().FranchiseID,
                FirstName = (CUST_FIRST_NAME ?? String.Empty).Capitalize(),
                LastName = (CUST_LAST_NAME ?? String.Empty).Capitalize()
            };

            var zipRepo = new ZipCodeRepository();
            var zip1 = zipRepo.Get(STOP1_ZIP);
            var zip2 = zipRepo.Get(STOP4_ZIP);

            if (zip1 == null)
            {
                zip1 = zipRepo.GetByFirst3(STOP1_ZIP.Substring(0, 3));
            }

            if (zip1 == null)
            {
                zip1 = zipRepo.GetDefault();
            }

            zip2 = zip2 ?? zipRepo.GetByFirst3(STOP4_ZIP.Substring(0, 3)) ?? zipRepo.GetDefault();

            var address = new Business.Models.Address() {
                Street1 = String.Empty,
                City = zip1.City,
                State = zip1.State,
                Zip = zip1.Zip
            };

            account.SetAddress(address, Business.Models.AddressType.Billing);
            account.SetAddress(address, Business.Models.AddressType.Mailing);

            accountRepo.Save();

            var numbers = new Dictionary<Business.Models.PhoneNumberType, string>() {
                { Business.Models.PhoneNumberType.Primary, CUST_PRIMARY_PHONE },
                { Business.Models.PhoneNumberType.Secondary, CUST_MOBILE_PHONE },
                { Business.Models.PhoneNumberType.Fax, CUST_FAX_PHONE }
            };

            Business.Models.PhoneNumber validNumber = null;
            bool primaryFailed = true;
            foreach (var num in numbers)
            {
                if (!String.IsNullOrEmpty(num.Value))
                {
                    var phone = new Business.Models.PhoneNumber(num.Value);
                    if (phone.IsValid())
                    {
                        validNumber = phone;
                        account.SetPhoneNumber(phone, num.Key);
                        if (num.Key == Business.Models.PhoneNumberType.Primary)
                        {
                            primaryFailed = false;
                        }
                    }
                }
            }

            if (primaryFailed && validNumber != null)
            {
                account.SetPhoneNumber(new Business.Models.PhoneNumber(validNumber.Number), Business.Models.PhoneNumberType.Primary);
            }
            else if (validNumber == null)
            {
                account.SetPhoneNumber(new Business.Models.PhoneNumber("5555555555"), Business.Models.PhoneNumberType.Primary);
            }

            if (Business.Models.EmailAddress.IsEmailAddress(CUST_PRIMARY_EMAIL))
            {
                var email = new Business.Models.EmailAddress(CUST_PRIMARY_EMAIL);
                account.SetEmail(email, Business.Models.EmailAddressType.Primary);
            }

            var repo = new QuoteRepository();
            var quote = new Business.Models.Quote();

            var stop1json = StopJson.GetEmpty();
            stop1json.city = zip1.City;
            stop1json.state = zip1.State;
            stop1json.zip = zip1.Zip;
            stop1json.addressType = GetAddressType(STOP1_ACTBUILDING_TYPE);
            stop1json.sort = 0;

            var stop2json = StopJson.GetEmpty();
            stop2json.city = zip2.City;
            stop2json.state = zip2.State;
            stop2json.zip = zip2.Zip;
            stop2json.addressType = GetAddressType(STOP4_ACTBUILDING_TYPE);
            stop2json.sort = 1;

            quote.ReferralMethod = "1800moovers.com";
            quote.Stops.Add(new Business.Models.Stop(stop1json));
            quote.Stops.Add(new Business.Models.Stop(stop2json));

            quote.ShippingAccount = account;
            quote.Account = account;

            quote.CustomInventoryData = cust_notes;
            quote.GuaranteeData = new GuaranteedInfo() {
                Adjustments = 0,
                BasePrice = 0,
                GuaranteedPrice = 0
            };

            quote.FranchiseID = account.FranchiseID;
            quote.AccountManagerID = (Guid)Membership.GetUser(General.WebQuoteUser).ProviderUserKey;
            
            try {
                quote.MoveDate = new DateTime(int.Parse(EST_MOVE_DATE_YEAR), int.Parse(EST_MOVE_DATE_MONTH), int.Parse(EST_MOVE_DAY));
            }
            catch (Exception) {
                quote.MoveDate = DateTime.Today.AddDays(7);
            }

            quote.CustomInventoryData = cust_notes + "|||" + InventoryMeta + "|||" + InventoryData + "|||" + CustomInventoryMeta + "|||" + CustomInventoryData;

            repo.Add(quote);
            repo.Save();

            quote.AddInventoryFromExternal(InventoryData);
            repo.Save();

            SendExternalQuoteEmail(quote.QuoteID, account.AccountID);
            return Content("Added");
        }

        private void SendExternalQuoteEmail(Guid quoteid, Guid accountid)
        {
            var accountrepo = new AccountRepository();
            var account = accountrepo.Get(accountid);

            var quoteRepo = new QuoteRepository();
            var quote = quoteRepo.Get(quoteid);

            var userRepo = new aspnet_UserRepository();
            var defaultUser = userRepo.GetRandomSalesPerson();

            var dbEmail = account.GetEmail(Business.Models.EmailAddressType.Primary);
            if (dbEmail == null) {
                return;
            }

            var to = dbEmail.Email;
            var model = new WebquoteEmailModel() {
                To = to,
                From = defaultUser.aspnet_Membership.Email,
                AccountManager = defaultUser.aspnet_Users_Profile.First(),
                Account = quote.Account,
                Quote = quote,
                Franchise = quote.Franchise
            };

            var emailBody = RenderViewToString("Emails/WebquoteReceived", model);
            Email.SendLoggedEmail(quoteRepo, quote, model, "Your Moovers Web Quote", emailBody, EmailCategory.WebQuoteReceived, true);
        }
    }
}
