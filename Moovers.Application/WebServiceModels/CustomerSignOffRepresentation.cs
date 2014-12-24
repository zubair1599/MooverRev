// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="CustomerSignOffRepresentation.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;

    public class CustomerSignOffRepresentation
    {
        public Guid quote_id { get; set; }

        public Guid stop_id { get; set; }

        public Guid valuation_type_id { get; set; }

        public decimal insurance_value { get; set; }

        public string confirmation_email { get; set; }

        public bool email_receipt { get; set; }

        public bool feedback_survey { get; set; }

        public bool future_offers { get; set; }

        public string latitude { get; set; }

        public string longitude { get; set; }

        public SignatureRepresentation signature { get; set; }
    }
}