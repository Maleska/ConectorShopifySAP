using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class Items
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public int OnHand { get; set; } 
        public int Commited { get; set; }
        public int exists { get; set; }
        public string idShopify { get; set; }
        public string inventory_item_id { get; set; }
        public string available_adjustment { get; set; }
        public string WhsCode { get; set; }
    }
}
