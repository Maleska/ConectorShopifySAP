using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class customers
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set;}
        public string state { get; set; }   
        public string currency { get; set; }
        public string phone {  get; set; }
        public List<addresses> addresses { get; set; }
        public List<default_address> default_Address { get; set; }

    }
    internal class addresses
    {
        public string id { get; set; }
        public string customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set;}
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get;set; }
        public string zip { get; set; }
        public string phone { get; set;}
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        
    }
    internal class default_address
    {
        public string id { get; set; }
        public string customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
    }
}
