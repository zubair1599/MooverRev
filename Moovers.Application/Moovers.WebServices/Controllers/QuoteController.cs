// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuoteController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.Webservices.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using Business.Interfaces;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using Moovers.WebServices.Controllers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using WebServiceModels;

    public class QuoteController : ControllerBase
    {
        private readonly IMoverScheduleRepository _scheduleRepo;

        private readonly ICustomAuthenticationRepository _secretRepository;

        public QuoteController(IMoverScheduleRepository scheduleRepo, ICustomAuthenticationRepository secretRepository)
        {
            if (scheduleRepo == null)
            {
                throw new ArgumentNullException("scheduleRepo");
            }
            _scheduleRepo = scheduleRepo;
            _secretRepository = secretRepository;
        }

        private string NilGuid
        {
            get { return "00000000-0000-0000-0000-000000000000"; }
        }

        /// <summary />
        /// Get List of schedules for day/month/year
        [HttpGet]
        [Route("v1/quote/list/")]
        //[Authorize]
        public HttpResponseMessage Get()
        {
            DateTime? date = DateTime.Today;
            Employee employee = GetCurrentUser().Employee_aspnet_User_Rel.FirstOrDefault().Employee;
            IEnumerable<Quote> quotes = _scheduleRepo.GetScheduleForDay(employee, date.Value.Day, date.Value.Month, date.Value.Year);
            var franchiseRepo = new FranchiseRepository();
            Guid franchiseId = franchiseRepo.GetUserFranchise(employee.GetAspUserID());
            IEnumerable<QuoteRepresentation> quoteRepresesentation =
                quotes.Where(q => q.FranchiseID == franchiseId && !q.Stops.Any(s => s.QuoteStatus.Any(st => st.Status_Type == 8 || st.Status_Type == 9)))
                    .Select(i => new QuoteRepresentation(i));
            IOrderedEnumerable<QuoteRepresentation> quoteSorted = quoteRepresesentation.OrderBy(q => q.sort);
            //return new QuoteWrapper(quoteSorted.ToList());
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new { status = "success", title = "", message = "data found", data = new { quotes = quoteSorted } });
        }

        [HttpPost]
        [Route("v1/quote/update/")]
        public HttpResponse Post(FormDataCollection data)
        {
            object test = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data["quotes"])));
            var jObject = test as JObject;

            IEnumerable<QuoteRepresentation> quotes = null;

            if (jObject != null)
            {
                JToken tt = jObject.SelectToken("data").SelectToken("quotes");
                quotes = JsonConvert.DeserializeObject(tt.ToString(), typeof(IEnumerable<QuoteRepresentation>)) as IEnumerable<QuoteRepresentation>;
            }

            return null;
        }

        [HttpPost]
        [Route("v1/quote/update-address-conditions/")]
        public HttpResponseMessage UpdateAddressConditions(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);
                object data = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));
                var jObject = data as JObject;

                if (jObject == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "Failed", title = request, message = "json object is null", });
                }

                var stopRepresentation = JsonConvert.DeserializeObject(jObject.ToString(), typeof(StopRepresentation)) as StopRepresentation;

                if (stopRepresentation == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { status = "Failed", title = request, message = "stop object is null", });
                }

                var stopRepository = new StopRepository();

                Stop stop = string.IsNullOrEmpty(stopRepresentation.stop_id)
                    ? new Stop { QuoteID = Guid.Parse(stopRepresentation.quote_id) }
                    : stopRepository.GetWithAddress(Guid.Parse(stopRepresentation.stop_id));

                stop.QuoteID = Guid.Parse(stopRepresentation.quote_id);
                stop.StorageDays = stopRepresentation.storage_days;

                stop.Address.Street1 = stopRepresentation.address1;
                stop.Address.Street2 = stopRepresentation.address2;
                stop.Address.City = stopRepresentation.city;
                stop.Address.State = stopRepresentation.state;
                stop.Address.Zip = stopRepresentation.postal_code;

                stop.Floor = stopRepresentation.condition.floors;
                stop.WalkDistance = stopRepresentation.condition.walking;
                stop.AddressTypeID = (int)stopRepresentation.condition.address_type;
                stop.ElevatorTypeID = (int)stopRepresentation.condition.elevator;
                stop.InsideStairsCount = stopRepresentation.condition.stairs_inside;
                stop.OutsideStairsCount = stopRepresentation.condition.stairs_outside;
                stop.InsideStairsTypeID = (int)stopRepresentation.condition.inside_stairs_type;
                stop.OutsideStairsTypeID = (int)stopRepresentation.condition.outside_stairs_type;

                stop.Liftgate = stopRepresentation.condition.requires_liftgate;
                stop.ParkingTypeID = (int)stopRepresentation.condition.parking_type;
                stop.Dock = stopRepresentation.condition.dock_high;
                stop.ApartmentGateCode = stopRepresentation.condition.complex_gate_code;
                stop.ApartmentComplex = stopRepresentation.condition.apartment_complex;

                stop.ModifiedBy = GetCurrentUser().UserName;
                stop.ModifiedOn = DateTime.Now;

                stopRepository.Save(ApplicationType.Api);

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { status = "success", title = "", message = "data updated successfully", data = new { updated = true } });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/inventory-update")]
        //[Authorize]
        public HttpResponseMessage UpdateInventories(HttpRequestMessage request)
        {
            try
            {
                string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

                var data1 = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;

                var sb = new StringBuilder();
                sb.Append(data1);
                sb.Append(request.ToString());

                _secretRepository.LogRequest("patric", sb);

                string quoteid = data1["quote_id"].ToString();
                string stopid = data1["stop_id"].ToString();
                string updatetype = data1["update_type"].ToString();
                object roomsjson = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data1["rooms"].ToString())));
                IEnumerable<RoomRepresentation> rooms = null;

                if (roomsjson != null)
                {
                    rooms = JsonConvert.DeserializeObject(roomsjson.ToString(), typeof(IEnumerable<RoomRepresentation>)) as IEnumerable<RoomRepresentation>;
                }

                if (rooms == null)
                {
                    return null;
                }

                var stopRepository = new StopRepository();
                var roomRepository = new RoomRepository();

                IEnumerable<string> existingroomsids = stopRepository.Get(Guid.Parse(stopid)).GetRooms().Select(room => room.RoomID.ToString().ToLower());
                List<RoomRepresentation> newrooms = rooms.Where(representation => !existingroomsids.Contains(representation.room_id.ToLower())).ToList();
                newrooms.Count();

                foreach (RoomRepresentation roomRepresentation in newrooms)
                {
                    string temproomid = roomRepresentation.room_id;
                    var room = new Room() { Name = roomRepresentation.name, Description = roomRepresentation.description, StopID = Guid.Parse(stopid) };
                    roomRepository.Add(room);
                    roomRepository.Save(ApplicationType.Api);
                    rooms.First(representation => representation.room_id == temproomid).room_id = room.RoomID.ToString();
                    if (roomRepresentation.inventories != null && roomRepresentation.inventories.Any())
                    {
                        roomRepresentation.inventories.ForEach(representation => representation.roomid = room.RoomID.ToString());
                    }
                }

                List<Room_InventoryItem> allExistingItems = stopRepository.Get(Guid.Parse(stopid)).GetAllRoomsItems().ToList();
                List<RoomInventoryItemRepresentation> allrecievedItems = rooms.Where(i => i.inventories != null).SelectMany(items => items.inventories).ToList();

                List<Room_InventoryItem> deletedItems =
                    allExistingItems.Where(
                        id =>
                            allExistingItems.Select(itemid => itemid.RelationshipID)
                                .ToList()
                                .Except(allrecievedItems.Select(ritemid => Guid.Parse(ritemid.relationshipid)).ToList())
                                .Contains(id.RelationshipID)).ToList();

                foreach (Room_InventoryItem roomInventoryItem in deletedItems)
                {
                    Room room = roomRepository.Get(roomInventoryItem.RoomID);

                    room.Room_InventoryItems.DeleteAll(item => item.RelationshipID == roomInventoryItem.RelationshipID);

                    allExistingItems.Remove(roomInventoryItem);
                }

                roomRepository.Save(ApplicationType.Api);

                List<RoomInventoryItemRepresentation> newCustomItems = allrecievedItems.Where(newitem => newitem.item_id == NilGuid).ToList();
                var repo = new InventoryItemRepository();

                foreach (RoomInventoryItemRepresentation roomInventoryItemRepresentation in newCustomItems)
                {
                    var item = new InventoryItem
                    {
                        Name = roomInventoryItemRepresentation.item.name,
                        PluralName = roomInventoryItemRepresentation.item.name,
                        CubicFeet = roomInventoryItemRepresentation.item.cubic_feet,
                        Weight = roomInventoryItemRepresentation.item.cubic_feet * 7,
                        MoversRequired = roomInventoryItemRepresentation.item.moovers_required,
                        IsCustom = true,
                        CreatedBy = GetCurrentUser().UserName,
                        CreatedOn = DateTime.Now
                    };

                    repo.Add(item);
                    repo.Save(ApplicationType.Api);

                    roomInventoryItemRepresentation.item_id = item.ItemID.ToString();
                }

                List<RoomInventoryItemRepresentation> newItems =
                    allrecievedItems.Where(
                        id =>
                            allrecievedItems.Select(ritemid => ritemid.relationshipid)
                                .ToList()
                                .Except(allExistingItems.Select(itemid => itemid.RelationshipID.ToString()).ToList())
                                .Contains(id.relationshipid)).ToList();

                foreach (RoomInventoryItemRepresentation roomInventoryItemRepresentation in newItems)
                {
                    Room room = roomRepository.Get(Guid.Parse(roomInventoryItemRepresentation.roomid));

                    var newroominventoryitem = new Room_InventoryItem
                    {
                        InventoryItemID = Guid.Parse(roomInventoryItemRepresentation.item_id),
                        Count = roomInventoryItemRepresentation.count,
                        StorageCount = roomInventoryItemRepresentation.storagecount,
                        CreatedBy = GetCurrentUser().UserName,
                        CreatedOn = DateTime.Now
                    };

                    if (roomInventoryItemRepresentation.addendums != null && roomInventoryItemRepresentation.addendums.Any())
                    {
                        foreach (RoomItemAddendum roomItemAddendum in roomInventoryItemRepresentation.addendums)
                        {
                            var roomitemoption = new Room_InventoryItem_Option
                            {
                                RelationshipID = Guid.Parse(roomItemAddendum.room_item_id),
                                QuestionID = Guid.Parse(roomItemAddendum.addendum_id),
                                CreatedBy = GetCurrentUser().UserName,
                                CreatedOn = DateTime.Now
                            };

                            if (!string.IsNullOrEmpty(roomItemAddendum.sub_addendum_id) && roomItemAddendum.sub_addendum_id != NilGuid)
                            {
                                roomitemoption.Option = Guid.Parse(roomItemAddendum.sub_addendum_id);
                            }

                            newroominventoryitem.Room_InventoryItem_Option.Add(roomitemoption);
                        }
                    }

                    if (roomInventoryItemRepresentation.notes != null && roomInventoryItemRepresentation.notes.Any())
                    {
                        foreach (RoomInventoryItemNoteRepresentation roomInventoryItemNote in roomInventoryItemRepresentation.notes)
                        {
                            newroominventoryitem.RoomInventoryItemNotes.Add(
                                new RoomInventoryItemNote()
                                {
                                    NoteText = roomInventoryItemNote.note_text,
                                    DateCreated = roomInventoryItemNote.stampdatetime,
                                    CreatedBy = GetCurrentUser().UserName,
                                    CreatedOn = DateTime.Now
                                    //RoomInventoryItemId = Guid.Parse(roomInventoryItemNote.note_room_inventory_item_id)
                                });
                        }
                    }

                    room.Room_InventoryItems.Add(newroominventoryitem);
                }

                roomRepository.Save(ApplicationType.Api);

                foreach (Room_InventoryItem roomInventoryItem in allExistingItems)
                {
                    RoomInventoryItemRepresentation item =
                        allrecievedItems.FirstOrDefault(itemid => Guid.Parse(itemid.relationshipid) == roomInventoryItem.RelationshipID);
                    if (item != null)
                    {
                        Room room = roomRepository.Get(roomInventoryItem.RoomID);

                        if (roomInventoryItem.RoomID != Guid.Parse(item.roomid))
                        {
                            roomRepository.Get(roomInventoryItem.RoomID)
                                .Room_InventoryItems.DeleteAll(rmvitem => rmvitem.RelationshipID == roomInventoryItem.RelationshipID);

                            Room newRoom = roomRepository.Get(Guid.Parse(item.roomid));

                            var newitme = new Room_InventoryItem
                            {
                                Count = item.count,
                                StorageCount = item.storagecount,
                                InventoryItemID = Guid.Parse(item.item_id),
                                CreatedBy = GetCurrentUser().UserName,
                                CreatedOn = DateTime.Now
                            };

                            if (item.addendums != null && item.addendums.Any())
                            {
                                foreach (RoomItemAddendum roomItemAddendum in item.addendums)
                                {
                                    var roomitemoption = new Room_InventoryItem_Option
                                    {
                                        RelationshipID = Guid.Parse(roomItemAddendum.room_item_id),
                                        QuestionID = Guid.Parse(roomItemAddendum.addendum_id),
                                        CreatedBy = GetCurrentUser().UserName,
                                        CreatedOn = DateTime.Now
                                    };

                                    if (!string.IsNullOrEmpty(roomItemAddendum.sub_addendum_id) && roomItemAddendum.sub_addendum_id != NilGuid)
                                    {
                                        roomitemoption.Option = Guid.Parse(roomItemAddendum.sub_addendum_id);
                                    }

                                    newitme.Room_InventoryItem_Option.Add(roomitemoption);
                                }
                            }

                            if (item.notes != null && item.notes.Any())
                            {
                                foreach (RoomInventoryItemNoteRepresentation roomInventoryItemNote in item.notes)
                                {
                                    newitme.RoomInventoryItemNotes.Add(
                                        new RoomInventoryItemNote
                                        {
                                            NoteText = roomInventoryItemNote.note_text,
                                            DateCreated = roomInventoryItemNote.stampdatetime,
                                            CreatedBy = GetCurrentUser().UserName,
                                            CreatedOn = DateTime.Now
                                            //RoomInventoryItemId = Guid.Parse(roomInventoryItemNote.note_room_inventory_item_id)
                                        });
                                }
                            }

                            newRoom.Room_InventoryItems.Add(newitme);
                        }
                        else
                        {
                            Room_InventoryItem existngitem =
                                room.Room_InventoryItems.FirstOrDefault(existingitem => existingitem.RelationshipID == Guid.Parse(item.relationshipid));
                            existngitem.Count = item.count;
                            existngitem.StorageCount = item.storagecount;
                            existngitem.ModifiedBy = GetCurrentUser().UserName;
                            existngitem.ModifiedOn = DateTime.Now;

                            if (item.addendums == null || !item.addendums.Any())
                            {
                                existngitem.Room_InventoryItem_Option.DeleteAll();
                            }
                            else
                            {
                                existngitem.Room_InventoryItem_Option.DeleteAll(
                                    it => !item.addendums.Select(adid => Guid.Parse(adid.room_addendum_id)).Contains(it.AdditionalInfoID));

                                foreach (RoomItemAddendum roomItemAddendum in item.addendums)
                                {
                                    if (existngitem.Room_InventoryItem_Option.All(it => it.AdditionalInfoID != Guid.Parse(roomItemAddendum.room_addendum_id)))
                                    {
                                        var roomitemoption = new Room_InventoryItem_Option
                                        {
                                            RelationshipID = Guid.Parse(roomItemAddendum.room_item_id),
                                            QuestionID = Guid.Parse(roomItemAddendum.addendum_id),
                                            CreatedBy = GetCurrentUser().UserName,
                                            CreatedOn = DateTime.Now
                                        };

                                        if (!string.IsNullOrEmpty(roomItemAddendum.sub_addendum_id) && roomItemAddendum.sub_addendum_id != NilGuid)
                                        {
                                            roomitemoption.Option = Guid.Parse(roomItemAddendum.sub_addendum_id);
                                        }

                                        existngitem.Room_InventoryItem_Option.Add(roomitemoption);
                                    }
                                }
                            }

                            if (item.notes != null && item.notes.Any())
                            {
                                foreach (RoomInventoryItemNoteRepresentation roomInventoryItemNote in item.notes)
                                {
                                    if (!existngitem.RoomInventoryItemNotes.Select(noteid => noteid.NoteId).Contains(Guid.Parse(roomInventoryItemNote.note_id)))
                                    {
                                        existngitem.RoomInventoryItemNotes.Add(
                                            new RoomInventoryItemNote
                                            {
                                                NoteText = roomInventoryItemNote.note_text,
                                                DateCreated = roomInventoryItemNote.stampdatetime,
                                                CreatedBy = GetCurrentUser().UserName,
                                                CreatedOn = DateTime.Now
                                                //RoomInventoryItemId = Guid.Parse(roomInventoryItemNote.note_room_inventory_item_id)
                                            });
                                    }
                                }
                            }
                        }
                    }
                }

                roomRepository.Save(ApplicationType.Api);

                var quoteRepo = new QuoteRepository();
                Quote quote = quoteRepo.Get(Guid.Parse(quoteid));
                quote = quoteRepo.UpdateSavedItemList(quote);
                quoteRepo.UpdateGuaranteedPrice(quote.QuoteID);
                quoteRepo.Save(ApplicationType.Api);

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { status = "success", title = "", message = "data updated successfully", data = new { updated = true } });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/customer-signoff/")]
        public HttpResponseMessage UpdateCustomer_SignOff(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                object jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));

                CustomerSignOffRepresentation customerSignOff = null;

                if (jObject != null)
                {
                    customerSignOff = JsonConvert.DeserializeObject(jObject.ToString(), typeof(CustomerSignOffRepresentation)) as CustomerSignOffRepresentation;
                }

                if (customerSignOff != null)
                {
                    IVerificationRepository repo = new VerificationRepository();

                    var signOff = new QuoteCustomerSignOff
                    {
                        QuoteID = customerSignOff.quote_id,
                        StopID = customerSignOff.stop_id,
                        ConfirmationEmail = customerSignOff.confirmation_email,
                        EmailReceipt = customerSignOff.email_receipt,
                        FeedbackSurvey = customerSignOff.feedback_survey,
                        FutureOffers = customerSignOff.future_offers,
                        ValuationTypeID = customerSignOff.valuation_type_id,
                        ValuationValue = customerSignOff.insurance_value,
                        Latitude = customerSignOff.latitude,
                        Longitude = customerSignOff.longitude,
                        AccountSignature =
                            new AccountSignature
                            {
                                SignatureTime = customerSignOff.signature.signature_time,
                                Type = customerSignOff.signature.signature_type,
                                Signature = customerSignOff.signature.signature
                            }
                    };

                    var quoterepo = new QuoteRepository();
                    Quote quote = quoterepo.Get(signOff.QuoteID);
                    if (quote != null)
                    {
                        quote.ValuationTypeID = signOff.ValuationTypeID;
                        quote.ReplacementValuationCost = signOff.ValuationValue;
                        quoterepo.Save(ApplicationType.Api);
                    }

                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            status = "success",
                            title = "",
                            message = "data updated successfully",
                            data = new { updated = repo.UpdateQuoteCustomerSignature(signOff) }
                        });
                }

                return null;
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/load/images/")]
        public async Task<HttpResponseMessage> Post()
        {
            try
            {
                LogRequest(Request, Request.RequestUri.ToString());
                LogRequest(Request, Request.Headers.GetValues("quote_lookup").First());
                // Check whether the POST operation is MultiPart? 
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                string lookup = Request.Headers.GetValues("quote_lookup").First();

                string path = "~/QuoteUploads/" + lookup;
                CreateSubPath(path);

                // Prepare CustomMultipartFormDataStreamProvider in which our multipart form // data will be loaded. 
                string fileSaveLocation = HttpContext.Current.Server.MapPath(path);

                var provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
                var files = new List<string>();

                // Read all contents of multipart message into CustomMultipartFormDataStreamProvider. 
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (MultipartFileData file in provider.FileData)
                {
                    files.Add(Path.GetFileName(file.LocalFileName));
                }
                // Send OK Response along with saved file names to the client. 
                return Request.CreateResponse(HttpStatusCode.OK, new { status = "success", title = "", message = fileSaveLocation });
            }
            catch (Exception e)
            {
                return GetFaultMessage(e);
            }
        }

        [HttpPost]
        [Route("v1/quote/inventory-verify-signoff/")]
        public HttpResponseMessage UpdateInventoryVerificationSignOff(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                object jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));

                InventoryVerificationRepresentation inventoryVerification = null;

                if (jObject != null)
                {
                    inventoryVerification =
                        JsonConvert.DeserializeObject(jObject.ToString(), typeof(InventoryVerificationRepresentation)) as InventoryVerificationRepresentation;
                }

                if (inventoryVerification != null)
                {
                    IVerificationRepository repo = new VerificationRepository();

                    var signOff = new InventoryVerification
                    {
                        QuoteID = inventoryVerification.quote_id,
                        StopID = inventoryVerification.stop_id,
                        Emails = inventoryVerification.email_receipts,
                        Latitude = inventoryVerification.latitude,
                        Longitude = inventoryVerification.longitude,
                        AccountSignature =
                            new AccountSignature
                            {
                                SignatureTime = inventoryVerification.signature.signature_time,
                                Type = inventoryVerification.signature.signature_type,
                                Signature = inventoryVerification.signature.signature
                            }
                    };

                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            status = "success",
                            title = "",
                            message = "data updated successfully",
                            data = new { updated = repo.UpdateInventoryVerificationSignature(signOff) }
                        });
                }

                return null;
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/unload-verify-signoff/")]
        public HttpResponseMessage UpdateUnloadVerificationSignOff(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                object jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));

                UnloadVerficationRepresentation unloadVerification = null;

                if (jObject != null)
                {
                    unloadVerification =
                        JsonConvert.DeserializeObject(jObject.ToString(), typeof(UnloadVerficationRepresentation)) as UnloadVerficationRepresentation;
                }

                if (unloadVerification != null)
                {
                    IVerificationRepository repo = new VerificationRepository();

                    var signOff = new UnloadVerification
                    {
                        QuoteID = unloadVerification.quote_id,
                        StopID = unloadVerification.stop_id,
                        SurveyID = unloadVerification.survey_id,
                        Gratuity = unloadVerification.gratuity,
                        Latitude = unloadVerification.latitude,
                        Longitude = unloadVerification.longitude,
                        AccountSignature =
                            new AccountSignature
                            {
                                SignatureTime = unloadVerification.signature.signature_time,
                                Type = unloadVerification.signature.signature_type,
                                Signature = unloadVerification.signature.signature
                            }
                    };

                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            status = "success",
                            title = "",
                            message = "data updated successfully",
                            data = new { updated = repo.UpdateUnloadVerificationSignature(signOff) }
                        });
                }

                return null;
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/update-status/")]
        public HttpResponseMessage UpdateQuoteStatus(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                object jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));

                QuoteStatusRepresentation quoteStatus = null;

                if (jObject != null)
                {
                    quoteStatus = JsonConvert.DeserializeObject(jObject.ToString(), typeof(QuoteStatusRepresentation)) as QuoteStatusRepresentation;
                }

                if (quoteStatus != null)
                {
                    IVerificationRepository repo = new VerificationRepository();

                    var status = new QuoteStatu
                    {
                        StatusUpdateTime = quoteStatus.status_datetime,
                        StopId = quoteStatus.stop_id,
                        Status_Type = quoteStatus.status_type,
                        Latitude = quoteStatus.latitude,
                        Longitude = quoteStatus.longitude
                    };

                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new { status = "success", title = "", message = "data updated successfully", data = new { updated = repo.UpdateQuoteStatus(status) } });
                }

                return null;
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/update-location/")]
        public HttpResponseMessage UpdateQuoteLocation(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                object jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));

                QuoteDirectionRepresentation direction = null;

                if (jObject != null)
                {
                    direction = JsonConvert.DeserializeObject(jObject.ToString(), typeof(QuoteDirectionRepresentation)) as QuoteDirectionRepresentation;
                }

                if (direction != null)
                {
                    var repo = new QuoteRepository();
                    Quote quote = repo.Get(direction.quote_id);

                    var newDirection = new QuoteDirectionRepresentation
                    {
                        quote_id = quote.QuoteID,
                        latitude = direction.latitude,
                        longitude = direction.longitude,
                        checkin_time = direction.checkin_time
                    };

                    if (quote.QuoteMapDirections.Count > 0)
                    {
                        string dir = quote.QuoteMapDirections.First().Direction;
                        var dirlist = JsonConvert.DeserializeObject<List<QuoteDirectionRepresentation>>(dir);
                        dirlist.Add(newDirection);
                        quote.QuoteMapDirections.First().Direction = dirlist.SerializeToJson();
                        quote.QuoteMapDirections.First().CheckinTime = newDirection.checkin_time;
                    }
                    else
                    {
                        var list = new List<QuoteDirectionRepresentation> { newDirection };

                        quote.QuoteMapDirections.Add(new QuoteMapDirection { Direction = list.SerializeToJson() });
                    }

                    repo.Save(ApplicationType.Api);
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new { status = "success", title = "", message = "data updated successfully", data = new { updated = true } });
                }

                return null;
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        [HttpPost]
        [Route("v1/quote/update_checklist/")]
        public HttpResponseMessage UpdateCheckList(HttpRequestMessage request)
        {
            try
            {
                HttpContent content = request.Content;
                string jsonContent = content.ReadAsStringAsync().Result;
                string cleanContent = Uri.UnescapeDataString(jsonContent);

                var jObject = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", "")))) as JObject;

                IPadCheckList questionire = null;

                if (jObject != null)
                {
                    questionire = JsonConvert.DeserializeObject(jObject.ToString(), typeof(IPadCheckList)) as IPadCheckList;
                    questionire.QuoteId = Guid.Parse(jObject["quote_id"].ToString());
                }
                var repo = new QuoteRepository();
                Quote quote = repo.Get(questionire.QuoteId);
                quote.IPadCheckLists.Add(questionire);
                repo.Save(ApplicationType.Api);
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { status = "success", title = "", message = "data updated successfully", data = new { updated = true } });
            }
            catch (Exception ex)
            {
                return GetFaultMessage(ex);
            }
        }

        private static void LogRequest(HttpRequestMessage request, string data)
        {
            var sb1 = new StringBuilder();
            sb1.Append(data);
            sb1.Append(request.ToString());

            new EmployeeAuthenticationRepository(new CacheRepository()).LogRequest("patric", sb1);
        }

        private static void CreateSubPath(string path)
        {
            bool isExists = Directory.Exists(HttpContext.Current.Server.MapPath(path));

            if (!isExists)
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));
            }
        }

        private HttpResponseMessage GetFaultMessage(Exception ex)
        {
            int line = new StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    status = "Failed",
                    title = ex.Message + ":inner exception: " + ex.InnerException,
                    message = ex.StackTrace,
                    lineno = line,
                    data = new { updated = false }
                });
        }
    }
}