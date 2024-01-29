using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class Products
    {
       public product product { get; set; } 
    }

    internal class product
    {
        public string id { get; set; }
        public string title { get; set; }
        public variants variants { get; set; }
        public string body_html { get; set; }
    }
    internal class variants
    {
        public string product_id { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
        public string inventory_quantity { get; set; }  
        public string inventory_item_id { get; set; }
    }
}
