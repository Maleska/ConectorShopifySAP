using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class RecountItems
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; } 
        public string sku { get; set; }
        public int Commited { get; set; }
        public int OnOrder { get; set; }
        public int OnHand { get; set; }
    }
}
