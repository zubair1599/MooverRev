using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Enums;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using Business.Utility;

namespace MooversCRM.Controllers
{
    //[Authorize]
    public class PostingPageController : BaseControllers.SecureBaseController
    {
        private Dictionary<string, Guid> SavePost(Guid id, decimal postingHours, decimal driveHours, decimal packingMaterialsCost, decimal packingServiceCost, decimal storageFeesCost, decimal replacementValuation, string otherServices, string employees, string vehicles)
        {
            var repo = new PostingRepository();
            var post = repo.Get(id);
            if (!post.Schedule.Quote.CanUserRead(User.Identity.Name))
            {
                throw new HttpException(404, "Not found");
            }

            post.PostingHours = postingHours;
            post.DriveHours = driveHours;

            // Save Services
            post.AddService(Business.Models.ServiceType.PackingMaterials, packingMaterialsCost);
            post.AddService(Business.Models.ServiceType.PackingServices, packingServiceCost);
            post.AddService(Business.Models.ServiceType.StorageFees, storageFeesCost);
            post.Quote.ReplacementValuationCost = replacementValuation;

            var otherServicesJson = General.DeserializeJson<IEnumerable<Business.Models.QuoteServiceJson>>(otherServices ?? String.Empty);
            var ret = new Dictionary<string, Guid>();
            foreach (var item in otherServicesJson)
            {
                Guid serviceID;
                bool exists = Guid.TryParse(item.ServiceID, out serviceID);
                var serv = post.AddCustomService(serviceID, item.Description, item.Price);
                if (!exists)
                {
                    repo.Save();
                }

                ret.Add(item.ServiceID, serv.ServiceID);
            }

            var serviceRepo = new QuoteServiceRepository();
            var toRemove = post.Quote.QuoteServices.Where(i => (i.Type == (int)Business.Models.ServiceType.Other) && !ret.Select(kvp => kvp.Value).Contains(i.ServiceID)).ToList();
            foreach (var serv in toRemove)
            {
                serviceRepo.Remove(serviceRepo.Get(serv.ServiceID));
            }

            serviceRepo.Save();

            post.RemoveEmployees();
            var employeesJson = General.DeserializeJson<IEnumerable<Business.Models.Employee_Rel>>(employees ?? String.Empty);
            foreach (var employee in employeesJson)
            {
                post.AddEmployee(employee);
            }

            repo.Save();

            var vehiclesJson = General.DeserializeJson<IEnumerable<Business.Models.Vehicle_Rel>>(vehicles ?? String.Empty);
            post.RemoveVehicles();
            foreach (var vehicle in vehiclesJson)
            {
                post.AddVehicle(vehicle);
            }

            repo.Save();

            return ret;
        }

        [HttpPost]
        public ActionResult SaveGuaranteedPostData(Guid id, decimal postingHours, decimal driveHours, decimal packingMaterialsCost, decimal packingServiceCost, decimal storageFeesCost, decimal replacementValuation, string otherServices, string employees, string vehicles)
        {
            var repo = new PostingRepository();
            var post = repo.Get(id);

            if (post.IsComplete)
            {
                return Json(new Dictionary<string, Guid>());
            }

            var ret = SavePost(id, postingHours, driveHours, packingMaterialsCost, packingServiceCost, storageFeesCost, replacementValuation, otherServices, employees, vehicles);
            
            // price = guaranteed price + service cost
            var price = post.Schedule.Quote.GuaranteeData.GuaranteedPrice;
            var services = post.Quote.QuoteServices.Sum(i => i.Price);
            var valuation = post.Quote.ReplacementValuationCost ?? 0;
            post.Schedule.Quote.FinalPostedPrice = price + services + valuation;
            repo.Save();
            return Json(ret);
        }

        [HttpPost]
        public ActionResult SaveHourlyPostData(Guid id, decimal finalPostedPrice, decimal postingHours, decimal driveHours, decimal hourlyRate, decimal firstHourRate, decimal packingMaterialsCost, decimal packingServiceCost, decimal storageFeesCost,  decimal replacementValuation, string otherServices, string employees, string vehicles)
        {
            var repo = new PostingRepository();
            var post = repo.Get(id);

            if (post.IsComplete)
            {
                return Json(new Dictionary<string, Guid>());
            }

            post.Schedule.Quote.FinalPostedPrice = finalPostedPrice;
            var ret = SavePost(id, postingHours, driveHours, packingMaterialsCost, packingServiceCost, storageFeesCost, replacementValuation, otherServices, employees, vehicles);

            if (post.Quote.PricingType == QuotePricingType.Hourly)
            {
                post.Quote.HourlyData = new HourlyInfo() {
                    CustomerTimeEstimate = post.Quote.HourlyData.CustomerTimeEstimate,
                    FirstHourPrice = firstHourRate,
                    HourlyPrice = hourlyRate
                };

                post.PostingRate = hourlyRate;
                post.PostingFirstHourRate = firstHourRate;
            }

            repo.Save();
            return Json(ret);
        }

        [HttpPost]
        public ActionResult CompletePost(Guid id)
        {
            var repo = new PostingRepository();
            var posting = repo.Get(id);
            posting.MarkComplete(AspUserID);
            repo.Save();
            return Json(true);
        }

        [HttpPost]
        public ActionResult GetPostingJson(Guid id)
        {
            var repo = new PostingRepository();
            var posting = repo.Get(id);

            if (!posting.Quote.CanUserRead(User.Identity.Name))
            {
                return HttpNotFound();
            }

            return Json(posting.ToJsonObject());
        }
    }
}
