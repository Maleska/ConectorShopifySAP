using ConectorShopifySAP.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class OVSAP
    {
        public string DocNum { get; set; }
        public string DocEntry { get;set; }
        public string CardName { get; set; }    
        public string CardCode{ get; set; }
        //public string email { get; set; }
        //public string street { get; set; }
        public string Comments { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string GroupNum { get; set; }
        //public string U_RFCondiciones { get; set; }
        public string U_IdOrdersShopify { get; set; }
        //public string U_NameShopify { get; set; }
        public List<OVSAPDetail> DocumentLines { get; set; }
        
        //public string PaymentMethod { get; set; }
        public string NumAtCard { get; set; }
        public string U_UN { get; set; } = Settings.Default.UnidadNegocio;
        public double DocTotal { get; set; }
        public string DiscPrcnt { get; set; }
        public string U_CodPromo { get;set; }
        public string PaymentMethod { get; set; }
        public string U_IL_Timbrar { get; set; }    

    }
    internal class OVSAPDetail
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string BarCode { get; set; }
        //public string DiscountPercent { get; set; }
        public string Quantity { get; set; }
        public string WarehouseCode { get; set; }
        public string Price { get; set; }
        //public string LineTotal { get; set; }
        //public string TaxCode { get; set; }

    }
}
