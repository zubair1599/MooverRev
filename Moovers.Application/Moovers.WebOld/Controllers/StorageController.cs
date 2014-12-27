// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="StorageController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Web.Mvc;

    using Business.Enums;
    using Business.FirstData;
    using Business.JsonObjects;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using Moovers.WebModels;
    using Moovers.WebModels.Reports;

    using MooversCRM.Controllers.BaseControllers;

    //[Authorize(Roles = "Administrator")]
    public class StorageController : SecureBaseController
    {
        public ActionResult Index(
            string search = "",
            StorageSort sort = StorageSort.DaysOverdue,
            int page = 0,
            int size = 200,
            bool desc = true,
            bool cancelled = false)
        {
            ViewBag.Search = search;
            ViewBag.Cancelled = cancelled;
            search = (search ?? String.Empty).Trim().ToLower();
            var repo = new StorageWorkOrderRepository();
            IQueryable<StorageWorkOrder> items = (cancelled) ? repo.GetInactive(sort, desc) : repo.GetActive(sort, desc);

            if (!String.IsNullOrWhiteSpace(search))
            {
                var tmp =
                    items.Select(
                        i =>
                            new
                            {
                                personaccount = i.Account as PersonAccount,
                                businessaccount = i.Account as BusinessAccount,
                                workorder = i,
                                account = i.Account
                            });

                items =
                    tmp.Where(
                        i =>
                            (i.personaccount != null
                                ? (i.personaccount.FirstName + " " + i.personaccount.LastName).ToLower().Contains(search)
                                : i.businessaccount.Name.ToLower().Contains(search)) || i.workorder.Lookup.Contains(search) || i.account.Lookup.Contains(search))
                        .Select(i => i.workorder);
            }

            var paged = new PagedResult<StorageWorkOrder>(items, page, size);
            return View(new StorageList(paged, sort, desc));
        }

        public new ActionResult View(string id)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workOrder = repo.Get(id);

            if (workOrder == null)
            {
                return HttpNotFound();
            }

            var vaultRepo = new StorageVaultRepository();
            IQueryable<StorageVault> vaults = vaultRepo.GetAll();
            var model = new StorageWorkOrderModel(workOrder, vaults);
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeAmount(
            Guid workOrderID,
            decimal monthlyPayment,
            DateTime startDate,
            DateTime invoiceDate,
            bool automaticBilling,
            bool paperlessInvoices,
            bool emailReceipt)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(workOrderID);

            workorder.MonthlyPayment = monthlyPayment;
            workorder.StartDate = startDate;
            workorder.NextInvoiceDate = invoiceDate;
            workorder.IsAutomaticBilling = automaticBilling;
            workorder.EmailInvoices = paperlessInvoices;
            workorder.EmailReceipts = emailReceipt;
            repo.Save();

            return RedirectToAction("View", new { Controller = "Storage", id = workorder.Lookup });
        }

        public ActionResult JobNotes(string id, string notes)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);
            if (workorder == null)
            {
                return HttpNotFound();
            }

            workorder.JobNotes = notes;
            repo.Save();

            return RedirectToAction("View", new { id = id });
        }

        public ActionResult VaultHistory(string id)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            return View(workorder);
        }

        public ActionResult VaultDetails(string id)
        {
            var repo = new StorageVaultRepository();
            StorageVault vault = repo.Get(id);

            if (vault == null)
            {
                return HttpNotFound();
            }

            return View(vault);
        }

        [HttpPost]
        public ActionResult ScheduleMove(string id, DateTime moveDate, StorageQuoteType moveType)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            var franchiseRepo = new FranchiseRepository();
            Franchise franchise = franchiseRepo.GetStorage();

            var quote = new Quote()
            {
                Account = workorder.Account,
                ShippingAccount = workorder.Account,
                FranchiseID = franchise.FranchiseID,
                ReferralMethod = "Used Before",
                MoveDate = moveDate,
                AccountManagerID = AspUserID
            };

            var stop = new Stop { Address = franchise.Address.Duplicate() };

            quote.Stops.Add(stop);

            if (moveType == StorageQuoteType.MoveOut)
            {
                RoomJson roomJson = RoomJson.GetFromMooversStorage();
                roomJson.Items = workorder.GetInventory();

                var room = new Room(roomJson);
                stop.Rooms.Add(room);
            }

            var rel = new StorageWorkOrder_Quote_Rel { StorageWorkOrder = workorder, StorageQuoteType = moveType };
            quote.StorageWorkOrder_Quote_Rel.Add(rel);

            repo.Save();

            // quoteids aren't automatically populated -- fetch from database for redirect
            var quoteRepo = new QuoteRepository();
            Quote ret = quoteRepo.Get(quote.QuoteID);
            ret.AddLog(AspUserID, "Move from Storage Acct " + workorder.Lookup + " Created");
            quoteRepo.UpdateGuaranteedPrice(quote.QuoteID, 0, true);
            quoteRepo.Save();

            return RedirectToAction("Stops", new { Controller = "Quote", id = ret.Lookup });
        }

        public ActionResult CancelInvoice(Guid invoiceid)
        {
            var repo = new StorageInvoiceRepository();
            StorageInvoice invoice = repo.Get(invoiceid);
            invoice.IsCancelled = true;
            repo.Save();
            return RedirectToAction("View", new { id = invoice.StorageWorkOrder.Lookup });
        }

        [HttpPost]
        public ActionResult AddStorage(Guid hiddenAccountID, DateTime startDate, DateTime billingDate, decimal monthlyAmount)
        {
            var storage = new StorageWorkOrder
            {
                AccountID = hiddenAccountID,
                NextInvoiceDate = billingDate,
                StartDate = startDate,
                MonthlyPayment = monthlyAmount
            };

            var repo = new StorageWorkOrderRepository();
            repo.Add(storage);
            repo.Save();

            // lookups aren't automatically refreshed from DB
            string lookup = new StorageWorkOrderRepository().Get(storage.WorkOrderID).Lookup;
            return RedirectToAction("View", new { id = lookup, Controller = "Storage" });
        }

        public ActionResult ViewStatement(Guid id)
        {
            var repo = new StorageStatementRepository();
            StorageStatement statement = repo.Get(id);
            File file = statement.File;

            if (!String.IsNullOrEmpty(file.HtmlContent))
            {
                file.SavedContent = General.GeneratePdf(file.HtmlContent, PaperKind.Letter);
                file.HtmlContent = null;
                repo.Save();
            }

            return File(file.SavedContent, file.ContentType, file.Name);
        }

        public ActionResult WarehouseManagement()
        {
            var repo = new StorageZoneRepository();
            var vaultRepo = new StorageVaultRepository();
            var overStuffRepo = new StorageWorkOrder_InventoryItem_RelRepository();

            var model = new StorageWarehouseModel(repo.GetAll(), vaultRepo.GetAll(), overStuffRepo.GetOverstuffed());
            return View(model);
        }

        public ActionResult Vaults(VaultSort sort = VaultSort.VaultID, bool desc = false, int page = 0, int pageSize = 50)
        {
            var repo = new StorageVaultRepository();
            var model = new VaultListModel(repo.GetAll(sort, desc, page, pageSize), sort, desc);
            return View(model);
        }

        public ActionResult ManageVault(string id, bool edit = false, string error = "")
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            var zoneRepo = new StorageZoneRepository();
            ViewBag.Zones = zoneRepo.GetAll();
            ViewBag.Edit = edit;

            if (!String.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
            }

            return View(workorder);
        }

        [HttpPost]
        public ActionResult AddVault(string id, string vaultid, Guid zoneid, string row, string shelf)
        {
            var repo = new StorageWorkOrderRepository();
            var vaultrepo = new StorageVaultRepository();

            StorageWorkOrder workorder = repo.Get(id);
            StorageVault vault = vaultrepo.Get(vaultid);

            string error = String.Empty;
            if (vault == null)
            {
                error = "Vault " + vaultid + " doesn't exist.";
            }
            else if (vault.IsUsed())
            {
                error = "Vault " + vaultid + " is in use by Work Order " + vault.GetWorkOrder().Lookup;
            }
            else
            {
                var rel = new StorageWorkOrder_Vault_Rel { ZoneID = zoneid, Row = row, Shelf = shelf, StorageVaultID = vault.StorageVaultID };
                workorder.StorageWorkOrder_Vault_Rel.Add(rel);
                repo.Save();
            }

            return RedirectToAction("ManageVault", new { id = id, error = error });
        }

        [HttpPost]
        public ActionResult RemoveVault(string id, Guid vaultid)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            StorageWorkOrder_Vault_Rel vault = workorder.StorageWorkOrder_Vault_Rel.FirstOrDefault(i => i.StorageVaultID == vaultid && !i.IsRemoved);
            if (vault != null)
            {
                vault.IsRemoved = true;
                vault.DateRemoved = DateTime.Now;
            }

            repo.Save();
            return RedirectToAction("ManageVault", new { id = id });
        }

        public ActionResult ManageInventory(string id)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);
            var itemRepo = new InventoryItemRepository();
            IQueryable<InventoryItem> items = itemRepo.GetUnarchived();

            var zoneRepo = new StorageZoneRepository();
            IEnumerable<StorageZone> zones = zoneRepo.GetAll();

            var model = new StorageInventoryModel(workorder, items, zones);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditOverstuffed(string id, Guid[] overstuffed, Guid[] isos, string[] row, string[] shelf, string[] alias, Guid[] oszone)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            for (int i = 0; i < overstuffed.Length; i++)
            {
                Guid itemid = overstuffed[i];
                string thisrow = row[i];
                string thisshelf = shelf[i];
                string thisalias = alias[i];
                Guid zoneid = oszone[i];
                bool thisisos = isos.Contains(itemid);

                if (thisisos)
                {
                    StorageWorkOrder_InventoryItem_Rel rel = workorder.StorageWorkOrder_InventoryItem_Rel.First(r => r.RelID == itemid);
                    rel.OverstuffRow = thisrow;
                    rel.OverstuffShelf = thisshelf;
                    rel.OverstuffZoneID = zoneid;
                    rel.OverstuffDescription = thisalias;
                    rel.IsOverstuffed = true;
                }
            }

            repo.Save();
            return RedirectToAction("ManageInventory", new { id = id });
        }

        [HttpPost]
        public ActionResult AddInventory(string id, Guid itemid, int count)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            workorder.AddInventory(itemid, count);
            repo.Save();
            return RedirectToAction("ManageInventory", new { id = id });
        }

        [HttpPost]
        public ActionResult RemoveInventory(string id, Guid relid)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            StorageWorkOrder_InventoryItem_Rel rel = workorder.StorageWorkOrder_InventoryItem_Rel.First(r => r.RelID == relid);
            rel.IsRemoved = true;
            repo.Save();

            return RedirectToAction("ManageInventory", new { id = id });
        }

        [HttpPost]
        public ActionResult AddCard(string id, ScheduleModal model)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(id);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            if (model.paymentType == "NEW_CARD" && !String.IsNullOrEmpty(model.cardnumber))
            {
                try
                {
                    model.cardnumber = model.cardnumber.Replace("-", String.Empty);

                    var franchiseRepo = new FranchiseRepository();
                    Franchise franchise = franchiseRepo.GetStorage();
                    var cardPayment = new CreditCardPayment();
                    TransactionResult token = cardPayment.GetCreditCardToken(
                        franchise,
                        model.name,
                        model.cardnumber,
                        model.expirationmonth,
                        model.expirationyear,
                        model.cvv2,
                        model.billingzip,
                        workorder.Account.Lookup);
                    if (!token.Transaction_Approved)
                    {
                        var error = new ErrorModel("cardnumber", "Invalid credit card number");
                        return Json(error);
                    }

                    Account_CreditCard rel = workorder.Account.AddCreditCard(franchise.FranchiseID, token);
                    workorder.Account_CreditCard = rel;
                    repo.Save();
                }
                catch (PaymentException e)
                {
                    return Json(new ErrorModel("cardnumber", e.Message));
                }
            }
            else if (!String.IsNullOrEmpty(model.paymentType))
            {
                Guid cardid = Guid.Parse(model.paymentType);
                var cardRepo = new Account_CreditCardRepository();
                Account_CreditCard card = cardRepo.Get(cardid);

                if (card.AccountID != workorder.AccountID)
                {
                    return new HttpStatusCodeResult(500, "Adding a credit card not associated with this account");
                }

                workorder.CreditCardID = card.CreditCardID;
                repo.Save();
            }

            workorder.ResetAutomaticBilling();
            repo.Save();

            return Json("Success");
        }

        public ActionResult PrintStatements()
        {
            var franchiseRepo = new FranchiseRepository();
            var workOrderRepo = new StorageWorkOrderRepository();

            DateTime forDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);

            List<StorageInvoice> invoices = workOrderRepo.GetForPrint().ToList();

            foreach (StorageInvoice i in invoices)
            {
                i.IsPrinted = true;
            }

            // order all distinct work orders in these invoices by name
            IEnumerable<StorageWorkOrder> workOrders =
                invoices.Select(i => i.StorageWorkOrder)
                    .Distinct()
                    .Where(i => i.NextInvoiceDate.HasValue)
                    .Select(i => new { person = i.Account as PersonAccount, business = i.Account as BusinessAccount, storage = i })
                    .OrderBy(i => i.person != null ? i.person.LastName : i.business.Name)
                    .Select(i => i.storage);

            // generate a statement for each separate work order, save a record of it.
            foreach (StorageWorkOrder workOrder in workOrders)
            {
                var statementModel = new StorageStatementModel()
                {
                    Date = forDate,
                    Franchise = franchiseRepo.GetStorage(),
                    StorageWorkOrders = new StorageWorkOrder[] { workOrder }
                };

                string html = RenderViewToString("PDFS/_StorageStatement", statementModel);
                workOrder.AddStatement(AspUserID, html);
                workOrderRepo.Save();
            }

            // generate a separate PDF to display all storage statements
            var model = new StorageStatementModel() { Date = forDate, Franchise = franchiseRepo.GetStorage(), StorageWorkOrders = workOrders };

            string content = RenderViewToString("PDFS/_StorageStatement", model);
            byte[] pdf = General.GeneratePdf(content, PaperKind.Letter);

            workOrderRepo.Save();
            return File(pdf, "application/pdf");
        }

        public ActionResult ViewInvoice(string id)
        {
            var repo = new StorageInvoiceRepository();
            StorageInvoice invoice = repo.Get(id);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            string html = RenderViewToString("PDFS/_Invoice", invoice);
            //return Content(html, "text/html");

            byte[] pdf = General.GeneratePdf(html, PaperKind.Letter);
            return File(pdf, "application/pdf");
        }

        public ActionResult ViewWorkOrder(string id)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workOrder = repo.Get(id);

            if (workOrder == null)
            {
                return HttpNotFound();
            }

            string view = RenderViewToString("PDFS/_WarehouseWorkOrder", workOrder);
            byte[] pdf = General.GeneratePdf(view, PaperKind.Letter);
            return File(pdf, "application/pdf");
        }

        [HttpPost]
        public ActionResult Cancel(Guid workorderID)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(workorderID);

            if (workorder == null)
            {
                return HttpNotFound();
            }

            workorder.Cancel(AspUser.UserId);
            repo.Save();

            return RedirectToAction("View", new { id = workorder.Lookup });
        }

        [HttpPost]
        public ActionResult AddPosting(string postid)
        {
            var postRepo = new PostingRepository();
            Posting posting = postRepo.Get(postid);

            if (posting == null)
            {
                return HttpNotFound();
            }

            var repo = new StorageWorkOrderRepository();
            var workOrder = new StorageWorkOrder { StartDate = DateTime.Now, AccountID = posting.Schedule.Quote.Account.AccountID };
            repo.Add(workOrder);
            repo.Save();

            workOrder.SetInventory(posting.Quote.GetItems());
            repo.Save();

            workOrder.AddQuote(posting.Quote, StorageQuoteType.MoveIn);
            repo.Save();

            IEnumerable<Quote> siblingQuotes = posting.Quote.Account.ShippingQuotes.Except(new[] { posting.Quote }).ToList().AsEnumerable();

            siblingQuotes = siblingQuotes.Where(q => !q.StorageWorkOrder_Quote_Rel.Any() && q.GetStops().Any(s => s.StorageDays == -1));

            if (siblingQuotes.Any())
            {
                workOrder.AddQuote(siblingQuotes.First(), StorageQuoteType.MoveOut);
            }

            repo.Save();

            // refresh lookup from DB
            string lookup = new StorageWorkOrderRepository().Get(workOrder.WorkOrderID).Lookup;
            return RedirectToAction("View", new { id = lookup });
        }

        public ActionResult ViewFile(Guid id)
        {
            var repo = new FileRepository();
            File file = repo.Get(id);
            return File(file.SavedContent, file.ContentType, file.Name);
        }

        public ActionResult UploadFile(string data, string contentType, string name, string storageid)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder storage = repo.Get(storageid);
            var fileRepo = new FileRepository();
            var file = new File { Name = name, ContentType = contentType };
            fileRepo.Add(file);
            fileRepo.Save();
            file.SavedContent = Convert.FromBase64String(data);
            fileRepo.Save();

            storage.AddFile(file);
            repo.Save();

            return Json(file.FileID);
        }

        public ActionResult ViewPaymentReceipt(string id)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder storage = repo.Get(id);
            ViewBag.Franchise = new FranchiseRepository().GetStorage();
            string view = RenderViewToString("PDFS/_PaymentReceipt", storage);

            var fileRepo = new FileRepository();
            var file = new File { Name = "Payment Receipt - " + DateTime.Now.ToShortDateString(), ContentType = "application/pdf", Created = DateTime.Now };
            fileRepo.Add(file);
            fileRepo.Save();

            byte[] pdf = General.GeneratePdf(view, PaperKind.Letter);
            file.SavedContent = pdf;
            fileRepo.Save();

            storage.AddFile(file);
            repo.Save();

            return File(pdf, "application/pdf", "receipt.pdf");
        }

        public ActionResult GetComments(Guid storageid)
        {
            var repo = new StorageWorkOrderRepository();
            return Json(repo.Get(storageid).GetComments().Select(i => i.ToJsonObject()));
        }

        [HttpPost]
        public ActionResult EditComment(Guid commentid, string text, bool delete = false)
        {
            var repo = new StorageCommentRepository();
            StorageComment comment = repo.Get(commentid);

            if (comment == null || (!IsAdministrator && !comment.IsEditable()))
            {
                return HttpNotFound();
            }

            if (delete)
            {
                repo.Delete(comment);
                repo.Save();
                return Json(true);
            }

            comment.Text = text;
            repo.Save();
            return Json(comment.ToJsonObject());
        }

        [HttpPost]
        public ActionResult AddComment(Guid storageid, string text)
        {
            var repo = new StorageWorkOrderRepository();
            StorageWorkOrder workorder = repo.Get(storageid);
            if (workorder == null)
            {
                return HttpNotFound();
            }

            StorageComment comment = workorder.AddComment(AspUserID, text);
            repo.Save();
            return Json(comment.ToJsonObject());
        }

        public ActionResult ViewStorageZone(Guid? zoneid, int? row)
        {
            var repo = new StorageVaultRepository();
            var zoneRepo = new StorageZoneRepository();
            IEnumerable<StorageZone> zones = zoneRepo.GetAll();
            if (zoneid.HasValue && row.HasValue)
            {
                IEnumerable<StorageVault> vaults = repo.GetInZone(zoneid.Value, row.ToString());
                return View(new StorageZoneReport(zones, vaults));
            }

            return View(new StorageZoneReport(zones));
        }

        public ActionResult SearchStorageVaults()
        {
            var repo = new StorageVaultRepository();
            return View();
        }

        [HttpPost]
        public ActionResult SearchStorageVaults(FormCollection fc)
        {
            string vaultNo = fc["vault_no"];
            var repo = new StorageVaultRepository();
            if (!String.IsNullOrEmpty(vaultNo))
            {
                StorageVault vault = repo.Get(vaultNo);
                return View(vault);
            }

            return View();
        }
    }
}