// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="HistoryRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using System.IO;
using Newtonsoft.Json;

namespace WebServiceModels
{
    using System;

    using Business.Models;

    public class InventoryItemHistory
    {
        public InventoryItemHistory(Quote_SavedItemList_Rel savedItem)
        {
            items_list = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(savedItem.ItemList)));
            last_updated = savedItem.Updated;
        }

        public InventoryItemHistory()
        {
        }

        public object items_list { get; set; }

        public DateTime last_updated { get; set; }
    }

    public class PricingAlgoHistory
    {
        public PricingAlgoHistory(Quote_PricingAlgorithm_Rel algo)
        {
            variable_list = algo.VariableList;
            algo_id = algo.PricingAlgorithmID.ToString();
            algo_text = algo.PricingAlgorithm.Text;
            added_date = algo.PricingAlgorithm.DateAdded.ToUniversalTime();
            algo_type_Id = algo.PricingAlgorithm.AlgorithmType;
        }

        public PricingAlgoHistory()
        {
        }

        public string variable_list { get; set; }

        public string algo_id { get; set; }

        public string algo_text { get; set; }

        public DateTime added_date { get; set; }
        public int algo_type_Id { get; set; } 
    }
}