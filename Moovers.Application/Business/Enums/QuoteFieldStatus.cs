using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Business.Enums
{
    public enum QuoteFieldStatus
    {
        [Description("Start")] Start,
        [Description("Drive towards stop")] DriveTowardsStop,
        [Description("Arrive at stop")] ArriveAtStop,
        [DescriptionAttribute("Inventory verification")] InventoryVerfication,
        [DescriptionAttribute("Customer approval")] CustomerApproval,
        [DescriptionAttribute("Load Truck")] LoadTruck,
        [Description("Loaded Customer approval")] LoadedCustomerApproval,
        [Description("Unload")] Unload,
        [Description("Customer approval")] CustomerArroval,
        [Description("Job ended")]
        JobEnded

    }
}
