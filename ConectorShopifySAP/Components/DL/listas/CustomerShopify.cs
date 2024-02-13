using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class CustomerShopify
    {
        
        public detalle customer { get; set; }


                //query = @"{\"customer\": { \"email\":\"" + email + "\"," +
                //            "\"first_name\": \"" + Nombre + "\"," +
                //            "\"last_name\": \"" + Apellido + "\"," +
                //            "\"phone\" : \""+ phone +"\"}}";
    }
    internal class detalle
    {
        public string email { get; set; }
        public string first_name { get; set;}
        public string last_name { get; set;}
        public string phone { get; set;}
    }
}
