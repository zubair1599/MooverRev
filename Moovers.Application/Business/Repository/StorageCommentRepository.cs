// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="StorageCommentRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;

    using Business.Models;

    public class StorageCommentRepository : RepositoryBase<StorageComment>
    {
        private Func<MooversCRMEntities, Guid, StorageComment> CompiledGetByID =
            CompiledQuery.Compile<MooversCRMEntities, Guid, StorageComment>((db, id) => db.StorageComments.SingleOrDefault(i => i.StorageCommentID == id));

        public override StorageComment Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public IEnumerable<StorageComment> GetForWorkOrder(Guid workorderId)
        {
            return db.StorageComments.Where(i => i.WorkOrderID == workorderId).OrderByDescending(i => i.Date);
        }

        public void Delete(StorageComment comment)
        {
            db.StorageComments.DeleteObject(comment);
        }
    }
}