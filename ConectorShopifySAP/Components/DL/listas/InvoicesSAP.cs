using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class InvoicesSAP
    {
        public string cardname { get; set; }
        public string cardcode { get; set; }
        public string email { get; set; }
        public string street { get; set; }
        public List<DetailInvoicesSAP> detailInvoices { get; set; }
    }
    internal class DetailInvoicesSAP
    {
        public string itemcode { get; set; }
        public string itemname { get; set; }
        public string sku { get; set; }
        public string disount { get; set; }
        public string price { get; set; }
        public string total { get; set; }
        public string tax { get; set; }
    }
}
