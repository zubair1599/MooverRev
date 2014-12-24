using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class InventoryItemRepository : RepositoryBase<InventoryItem>
    {
        private static readonly Func<MooversCRMEntities, Guid, InventoryItem> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, InventoryItem>(
            (db, id) => db.InventoryItems.SingleOrDefault(i => i.ItemID == id)
            );

        private static readonly Func<MooversCRMEntities, IQueryable<InventoryItem>> CompiledGetUnarchived = CompiledQuery.Compile<MooversCRMEntities, IQueryable<InventoryItem>>(
            (db) => (db.InventoryItems.Include("InventoryItemQuestions").Include("InventoryItemQuestions.InventoryItemQuestionOptions").Where(i => !i.IsArchived && !i.IsCustom).OrderBy(i => i.Name))
            );

        private static readonly Func<MooversCRMEntities, Guid, IQueryable<InventoryItem>> CompiledGetForQuote = CompiledQuery.Compile<MooversCRMEntities, Guid, IQueryable<InventoryItem>>(
            (db, quoteid) => (db.Quotes.Where(q => q.QuoteID == quoteid).SelectMany(q => (from s in q.Stops
                from r in s.Rooms
                from rel in r.Room_InventoryItems
                where rel.InventoryItem.IsCustom
                select rel.InventoryItem)))
            );

        private static readonly Func<MooversCRMEntities, int, InventoryItem> CompiledGetByMovepointKeycode = CompiledQuery.Compile<MooversCRMEntities, int, InventoryItem>(
            (db, keycode) => db.InventoryItems.FirstOrDefault(i => i.MovepointKeycode == keycode)
            );

        public InventoryItem GetByMovepointKeycode(int keycode)
        {
            return CompiledGetByMovepointKeycode(db, keycode);
        }

        public override InventoryItem Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public InventoryItem GetByKeyCode(int code)
        {
            return db.InventoryItems.SingleOrDefault(i => i.KeyCode == code);
        }

        public void Remove(InventoryItem item)
        {
            item.IsArchived = true;
        }

        public IQueryable<InventoryItem> GetUnarchived()
        {
            return CompiledGetUnarchived(db);
        }

        public IEnumerable<InventoryItem> GetCustomForQuote(Guid quoteid)
        {
            return CompiledGetForQuote(db, quoteid);
        }

        public Dictionary<InventoryItem, int> GetItemCounts(int take = 40)
        {
            var items = (from i in db.InventoryItems
                where !i.IsCustom
                      && !i.IsArchived
                select i);

            return items.ToDictionary(i => i, i => i.Room_InventoryItem.Sum(r => r.Count)).OrderByDescending(i => i.Value).Take(take).ToDictionary(i => i.Key, i => i.Value);
        }

        public Dictionary<string, int> GetCustomItemCounts(int take = 50)
        {
            var items = (from i in db.InventoryItems
                where i.IsCustom
                      && !i.IsArchived
                select i.Name);

            return items.GroupBy(i => i).OrderByDescending(i => i.Count()).Take(take).ToDictionary(i => i.Key, i => i.Count());
        }
    }
}