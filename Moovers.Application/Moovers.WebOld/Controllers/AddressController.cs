using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business;
using Business.Repository.Models;
using Moovers.WebModels;

namespace MooversCRM.Controllers
{
    using System.Web.Script.Serialization;

    using Business.Enums;
    using Business.Utility;

    using Newtonsoft.Json;

    [Authorize]
    public class AddressController : BaseControllers.NonSecureBaseController
    {
        [HttpPost]
        public ActionResult GetSuggestions(Business.Models.Address address)
        {            
            var candidates = Enumerable.Empty<CandidateAddress>();
            if (!String.IsNullOrEmpty(address.Street1))
            {
                candidates = Business.Utility.AddressVerification.GetCandidates(address);
            }

            return Json(candidates);
        }

        [HttpPost]
        public ActionResult VerifyZipCode(string zip)
        {
            var repo = new ZipCodeRepository();

            var zipCode = repo.Get(zip);
            if (zipCode == null)
            {
                var errorModel = new ErrorModel("zip", "Couldn't find the zip");
                return Json(errorModel);
            }

            var latLng1 = new LatLng() { Latitude = (double)zipCode.Latitude, Longitude = (double)zipCode.Longitude };
            var latLng2 = new LatLng() { Latitude = Business.Utility.DistanceCalculator.KansasCityLatitude, Longitude = Business.Utility.DistanceCalculator.KansasCityLongitude };

            try 
            {
                // before verifying an address, make sure we can get directions to/from it, because we use distance calculations a lot, and can't have them throwing errors
                latLng1.GetDistance(latLng2);
            }
            catch (Exception e)
            {
                var errorrepo = new ErrorRepository();
                errorrepo.Log(e, "VerifyZip", new System.Collections.Specialized.NameValueCollection(), new System.Collections.Specialized.NameValueCollection());
                errorrepo.Save();
            }

            return Content("Success");
        }

        [HttpGet]
        public ActionResult GetDistance(Guid address1ID, Guid address2ID)
        {
            var addressRepo = new AddressRepository();
            var address1 = addressRepo.Get(address1ID);
            var address2 = addressRepo.Get(address2ID);
            var distance = address1.GetLatLng().GetDistance(address2.GetLatLng());
            var time = address1.GetLatLng().GetTime(address2.GetLatLng());

            return Json(new {
                distance = Math.Ceiling(distance),
                time = Math.Ceiling(time)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllStates()
        {
            var state = General.StateDictionary;
            var json = new JavaScriptSerializer().Serialize(state);
            //string json = JsonConvert.SerializeObject(state);
            return Json(json, JsonRequestBehavior.AllowGet);
        
        }


        [HttpGet]
        public JsonResult AddressTypes(string type)
        {
            var allAddress = StopAddressType.Apartment.ToDictionary(type);
            var json = new { buildings= allAddress.SerializeToJson()};
            //string json = JsonConvert.SerializeObject(state);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ParkingDriveway()
        {
            var driveway = ParkingType.Driveway.ToDictionary();

            var json = new { drive = driveway.SerializeToJson() };
            //string json = JsonConvert.SerializeObject(state);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}
