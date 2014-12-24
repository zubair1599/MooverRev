using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using Business.Enums;
using Business.JsonObjects;
using Business.Models;
using Business.Utility;
using Moovers.Webservices.Controllers;
using Newtonsoft.Json.Serialization;
using WebGrease.Css.Extensions;
using WebServiceModels;


namespace Moovers.WebServices.Controllers
{
    public class GeneralController : ControllerBase
    {
        public List<EnumRepresentation> enums { get; set; }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/general/code/")]
        public HttpResponseMessage Get()
        {
            enums = new List<EnumRepresentation>();

            AddEnums();


            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    status = "success",
                    title = "",
                    message = "data found",

                    data = new
                    {
                        codes = enums
                    }
                });
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/general/inventories/")]
        public HttpResponseMessage GetInventories()
        {
            var inventoryItems = new List<InventoryItemRepresentation>();
            var items = new Business.Repository.Models.InventoryItemRepository().GetUnarchived();
            items.ForEach(i =>
            {
               
                var item = new InventoryItemRepresentation
                {
                    name = i.Name,
                    Item_id = i.ItemID,
                    key_code =  i.KeyCode,
                    cubic_feet =  i.CubicFeet,
                    weight =  i.Weight,
                    custom_time = i.CustomTime,
                    moovers_required = i.MoversRequired,
                    is_box = i.IsBox,
                    is_special_item = i.IsSpecialItem(),
                    liability_cost = i.LiabilityCost               
                };
                i.InventoryItemQuestions.ForEach(q =>
                {
                    if (item.addendums == null)
                       item.addendums = new List<Addendum>();
                    item.addendums.Add(new Addendum(q));
                    q.InventoryItemQuestionOptions.ForEach(o => item.addendums.Add(new Addendum(o)));
                });
                inventoryItems.Add(item);
            });
            return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        status = "success",
                        title = "",
                        message = "data retrieved successfully",
                        data = new { inventories = inventoryItems }
                    });;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("v1/general/valuations/")]
        public HttpResponseMessage GetValuations()
        {
            var valuations = new List<object>();
            var items = new Business.Repository.Models.ReplacementValuationRepository().GetAll();
            items.ForEach(i =>
            {

                var item = new 
                {
                    Name = i.Name,
                    ValuationTypeID = i.ValuationTypeID,
                     MaximumValue = i.MaximumValue,
                    PerPound = i.PerPound,
                    Cost = i.Cost,
                    Type = i.Type  
                };

                valuations.Add(item);
            });
            return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        status = "success",
                        title = "",
                        message = "data retrieved successfully",
                        data = new { valuations = valuations }
                    }); 
        }

        private void AddEnums()
        {
            EnumLoop<CrewStatus>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int) c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<ElevatorType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<ParkingType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<PaymentTypes>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<QuotePricingType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<QuoteStatus>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<QuoteType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<StairType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<StopAddressType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<SignatureType>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<QuoteFieldStatus>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
            EnumLoop<AlgorithmTypes>.ForEach(c => enums.Add(new EnumRepresentation
            {
                key = (int)c,
                value = c.ToString(),
                display = c.GetDescription(),
                type = c.GetType().ToString()
            }));
        }
    }

    internal class EnumLoop<Key> where Key : struct, IConvertible
    {
        private static readonly Key[] arr = (Key[]) Enum.GetValues(typeof (Key));

        internal static void ForEach(Action<Key> act)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                act(arr[i]);
            }
        }
    }
}