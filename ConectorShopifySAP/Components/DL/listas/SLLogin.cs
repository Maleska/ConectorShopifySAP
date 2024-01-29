using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class SLLogin
    {
        public string SessionId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string SessionTimeout { get; set; } = string.Empty;
    }
}
