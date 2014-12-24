// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuoteCommentRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Data.Objects;
    using System.Linq;

    using Business.Models;

    public class QuoteCommentRepository : RepositoryBase<QuoteComment>
    {
        private Func<MooversCRMEntities, Guid, QuoteComment> CompiledGetByID =
            CompiledQuery.Compile<MooversCRMEntities, Guid, QuoteComment>((db, id) => db.QuoteComments.SingleOrDefault(i => i.QuoteCommentID == id));

        public override QuoteComment Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public void Delete(QuoteComment comment)
        {
            db.QuoteComments.DeleteObject(comment);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.QuoteComments.Where(i => i.UserID == userid);
            foreach (var item in items)
            {
                db.QuoteComments.DeleteObject(item);
            }
        }
    }
}