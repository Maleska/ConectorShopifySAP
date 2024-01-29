using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class OVShopify
    {
        public string idShopify { get; set; }
        public string name { get; set; }
        public string order_number { get; set; }
        public string customer { get; set; }
        public string created_at { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public List<detailOVShopify> ditails { get; set; }
        public discount_application discount { get; set; }
        public string customer_id { get; set; }
        public string financial_status { get; set;}
        public string payment_gateway_names { get; set; }

    }
    internal class detailOVShopify
    {
        public string sku { get; set; }
        public string dscription { get; set; }
        public string itemcode { get; set; }
        public string quantity { get; set; }
        public string price { get; set; }
        public string tax_line { get; set; }
        public string discount { get; set; }
    }
    internal class discount_application
    {
        public string target_type { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public string allocation_method { get; set; }
        public string target_selection { get; set; }
        public string code { get; set; }
    }
}
