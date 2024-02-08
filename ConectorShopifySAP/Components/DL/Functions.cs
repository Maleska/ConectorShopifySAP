using ConectorShopifySAP.Components.DL.listas;
using ConectorShopifySAP.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
//using System.Net.Http.Headers;
//using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using GraphQL;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Runtime.InteropServices;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Data.SqlClient;
using System.Net.WebSockets;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ConectorShopifySAP.Components.DL
{
    internal class Functions
    {
        #region Variables
            public static HanaConnection cnn = new HanaConnection(Settings.Default.HanaConection);
            public static HanaCommand cmd = null;
            public static string sQuery,ItemCode,CardCode,docnum,docentry = string.Empty;
            public static HanaDataReader reader = null;
            public static bool bandOrders = false;

        /// <SAP>
            public static Items valoresItems = new Items();
            public static List<Items> listaItems = new List<Items>();
            public static OVSAP ovsap = new OVSAP();
            public static Products product = new Products();
            public static ProductSAP productSAP = new ProductSAP();
            public static List<ProductSAP> listaProductSAP ;
            public static List<Products> listaProduct;
            public static List<customers> listCustomer;
        /// </summary>
        #endregion
        public static void ConexionSL()
        {
            try
            {
                //string data = "{\"CompanyDB\": \"" + DB + "\",    \"UserName\": \"Desarrollo\",       \"Password\": \"S4pb1234\"}";
                //string url = "https://hanab1:50000/b1s/v1/Login";

                //var data = @"{
                //            " + "\n" +
                //                @"    ""UserName"":\""+ Properties.Settings.Default.UserSAP+\"",
                //            " + "\n" +
                //                @"    ""Password"":""mana1"",
                //            " + "\n" +
                //                @"    ""CompanyDB"":""SB1PRUEBA""
                //            " + "\n" +
                //                @"}";

                var data = "{    \"CompanyDB\": \"" + Settings.Default.CompanyDB + "\",    \"UserName\": \"" + Settings.Default.UserSAP + "\",       \"Password\": \"" + Settings.Default.PassSAP + "\",\"Language\": \"25\"}";


                Log("DATA para services layer Conection: " + data + "" + DateTime.Now);

                var WebReq = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.urlSL + "/Login");
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //listas.SLLogin obj = null;
                Globals.sLLogin = new listas.SLLogin();
               

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    //Console.WriteLine(result);
                    var obj = JsonConvert.DeserializeObject<dynamic>(result);
                    //var sesion = JsonConvert.DeserializeObject<dynamic>(result);
                    WebReq.Headers.Add("Cookie", $"B1SESSION=" + obj["SessionId"]);
                    httpResponse.Headers.Add("Cookie", $"B1SESSION=" + obj["SessionId"]);
                    Globals.sLLogin.SessionId = obj["SessionId"];
                    Globals.sLLogin.Version = obj["Version"];
                    Globals.sLLogin.SessionTimeout = obj["SessionTimeout"];
                    Log("Conexion establecida SL: " + DateTime.Now);
                }
                //return obj;
            }
            catch (Exception ex)
            {

                //using (WebResponse response = ex.Response)
                //{
                //    // TODO: Handle response being null
                //    HttpWebResponse httpResponse = (HttpWebResponse)response;
                //    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                //    using (Stream data = response.GetResponseStream())
                //    using (var reader = new StreamReader(data))
                //    {
                //        var text = reader.ReadToEnd();
                //        //var model = JsonConvert.DeserializeObject<DL.listas.errores>(text);
                        Log(ex.Message.ToString());
                //    }
                //}
            }
        }
        public static void ConexionODO()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
        }
        public static void SaveOrderSAP_old(DL.listas.OVShopify lista)
        {
            try
            {
                var json = JsonConvert.SerializeObject(lista);

                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Cookie("B1SESSION", Globals.sLLogin.SessionId) { Domain = "localhost" });

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })

                    if (Globals.sLLogin.SessionId == null)
                    {
                        ConexionSL();
                    }

                    string url = Settings.Default.urlSL + Settings.Default.OrdersSAP;
                using (var httpclient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"),url))
                    {
                        //request.Headers.TryAddWithoutValidation(name: "SessionId", value: Globals.sLLogin.SessionId);
                        ServicePointManager.Expect100Continue = true;
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                        request.Headers.GetCookies("B1SESSION");
                        /*request.Headers.Add("Cookie", $"B1SESSION=" + Globals.sLLogin.SessionId);
                        request.Headers.Add("ROUTEID", ".node1");*/
                        request.Content = content;
                        //request.Headers.TryAddWithoutValidation(name: "", value: "");
                        httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = httpclient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.InnerException.InnerException.Message);
            }
        }
        public static void SaveOrderSAP(DL.listas.OVSAP lista, string estado)
        {
            try
            {

                Components.DL.Functions.ConexionSL();

                string url = Settings.Default.urlSL + Settings.Default.OrdersSAP;
                var json = JsonConvert.SerializeObject(lista);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                var WebReq = (HttpWebRequest)WebRequest.Create(url);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;
                WebReq.Headers.Add("Cookie", $"B1SESSION=" + Globals.sLLogin.SessionId);

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(json); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //listas.SLLogin obj = null;
                //Globals.sLLogin = new listas.SLLogin();
               
                if (httpResponse.StatusCode == HttpStatusCode.Created)
                {
                    var responseString = new
                        StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                     ovsap = JsonConvert.DeserializeObject<DL.listas.OVSAP>(responseString);
                    //var model = JsonConvert.DeserializeObject<DL.listas.errores>(text);
                    Log(DateTime.Now + "OV creada en SAP ");
                    lista.DocNum = ovsap.DocNum;
                    lista.DocEntry = ovsap.DocEntry;
                    //BL.Functions.CreateInvoices(ovsap.DocEntry, lista, estado);

                }
                else
                {
                    var responseString = new
                       StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                   errores err = JsonConvert.DeserializeObject<DL.listas.errores>(responseString);
                    //var model = JsonConvert.DeserializeObject<DL.listas.errores>(text);
                    Log(DateTime.Now + "Error al crear OV: " + err.error.message.value);
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    // TODO: Handle response being null
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        var text = reader.ReadToEnd();
                        var model = JsonConvert.DeserializeObject<DL.listas.errores>(text);
                        Log(model.error.message.value);
                    }
                }
            }
            
        }
        public static bool ValidateServerCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public static void getBusinessPartners(string CardCode)
        {
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                }

                sQuery = "select \"CardCode\" from \""+ Settings.Default.CompanyDB +"\".OCRD where \"CardCode\" = '" + CardCode + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    ItemCode = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static void getCustomerShopify(string id)
        {
            try
            {
                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" + 
                    Properties.Settings.Default.customerShopify + id + ".json";
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                    {
                        //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        JArray arr = (JArray)jsonObject["customer"];
                        var code = jsonObject["code"].ToString();

                        foreach (var item in jsonObject["customer"])
                        {

                            

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }   
        }
        public static void getItemCodeBySKU(string sku)
        {
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                }

                sQuery = "select \"ItemCode\" from OITM where \"CodeBars\" = '"+ sku +"'";
                cmd = new HanaCommand(sQuery,cnn);
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    ItemCode = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();

            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static bool getexistsOrders(string order_number,string nombre)
        {
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                    cnn.Open();
                }
                sQuery = "select \"DocNum\" from "+Settings.Default.CompanyDB+ ".ORDR where \"U_IdOrdersShopify\" = '" + 
                    order_number + "' and \"U_UN\" ='"+ Properties.Settings.Default.UnidadNegocio +"' and \"CANCELED\" = 'N'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();
                
                if (reader.HasRows)
                {
                    bandOrders = true;
                }
                reader.Close();
                //cnn.Close();

            }
            catch (Exception ex)
            {
                Log(ex.Message);
                bandOrders = false;
            }
            return bandOrders;
        }
        public static string getRowCountInvoice()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
            using (HanaCommand command = new HanaCommand("SELECT COUNT(\"ID\") FROM \"SYNCDB\".\"INVOICE\" order by \"ID\" desc", cnn))
            {
                return (string)command.ExecuteScalar();
            }
        }
        public static string getRowCountGoodsEntry()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
            using (HanaCommand command = new HanaCommand("SELECT COUNT(\"ID\") FROM \"SYNCDB\".\"ENTRYS\" where \"SYNC\" ='00' order by \"ID\" desc", cnn))
            {
                return (string)command.ExecuteScalar();
            }
        }
        public static string getRowCountInventoryTransfer()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
            using (HanaCommand command = new HanaCommand("SELECT COUNT(\"ID\") FROM \"SYNCDB\".\"TRANSFERS\" where \"SYNC\" = '00'", cnn))
            {
                return (string)command.ExecuteScalar().ToString();
            }
        }
        public static string getRowCountGoodsIssue()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
            using (HanaCommand command = new HanaCommand("SELECT COUNT(\"ID\") FROM \"SYNCDB\".\"GOODS_ISSUE\" where \"SYNC\" = '00'", cnn))
            {
                return (string)command.ExecuteScalar().ToString();
            }
        }   
        public static string getRowCountBP()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }
            using (HanaCommand command = new HanaCommand("SELECT COUNT(\"ID\") FROM \"SYNCDB\".\"BUSINESS_PARTNER\" where \"SYNC\" = '00'", cnn))
            {
                return (string)command.ExecuteScalar().ToString();
            }
        }
        //public static void getLastRegisterOINV(ref string docnum, ref string docentry,ref string id)
        public static List<listas.Items> RecalcInventoryInvoices(string docnum)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("select T1.\"ItemCode\",T1.\"Dscription\",T1.\"CodeBars\",cast(T1.\"Quantity\" as int),cast(T2.\"OnHand\" as int),cast(T2.\"IsCommited\" as int)from \"" + Properties.Settings.Default.CompanyDB + "\".oinv T0 ");
            sb.Append("inner join " + Properties.Settings.Default.CompanyDB + ".INV1 T1 on T0.\"DocEntry\" = T1.\"DocEntry\" ");
            sb.Append("Inner join " + Properties.Settings.Default.CompanyDB + ".OITW T2 on T1.\"ItemCode\" = T2.\"ItemCode\" ");
            sb.Append("where T0.\"DocNum\" = '" + docnum + "' and T1.\"WhsCode\" = "+ Settings.Default.WhsCode +"");

            using (HanaCommand command = new HanaCommand(sb.ToString(), cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        valoresItems.ItemCode = _reader.GetString(0).ToString();
                        valoresItems.ItemName = _reader.GetString(1).ToString();
                        valoresItems.SKU = _reader.GetString(2).ToString();
                        valoresItems.Quantity = int.Parse(_reader.GetString(3).ToString());
                        valoresItems.OnHand = int.Parse(_reader.GetString(4).ToString());
                        valoresItems.Commited = int.Parse(_reader.GetString(5).ToString());
                        valoresItems.exists = int.Parse(_reader.GetString(4).ToString()) - (int.Parse(_reader.GetString(3).ToString()) + int.Parse(_reader.GetString(5).ToString()));

                        listaItems.Add(valoresItems);
                        valoresItems = new Items();
                    }
                }
                _reader.Close();
                //cnn.Close();

                return listaItems;
            }
        }
        public static List<listas.Items> RecalcInventoryGoodsEntry(string docnum)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("select T1.\"ItemCode\",T1.\"Dscription\",T1.\"CodeBars\",cast(T1.\"Quantity\" as int),cast(T2.\"OnHand\" as int),cast(T2.\"IsCommited\" as int) from \"" + Properties.Settings.Default.CompanyDB + "\".OPDN T0 ");
            sb.Append("inner join " + Properties.Settings.Default.CompanyDB + ".PDN1 T1 on T0.\"DocEntry\" = T1.\"DocEntry\" ");
            sb.Append("Inner join " + Properties.Settings.Default.CompanyDB + ".OITW T2 on T1.\"ItemCode\" = T2.\"ItemCode\" ");
            sb.Append("where T0.\"DocNum\" = '" + docnum + "' and T1.\"WhsCode\" = "+ Settings.Default.WhsCode+"");

            using (HanaCommand command = new HanaCommand(sb.ToString(), cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        valoresItems.ItemCode = _reader.GetString(0).ToString();
                        valoresItems.ItemName = _reader.GetString(1).ToString();
                        valoresItems.SKU = _reader.GetString(2).ToString();
                        valoresItems.Quantity = int.Parse(_reader.GetString(3).ToString());
                        valoresItems.OnHand = int.Parse(_reader.GetString(4).ToString());
                        valoresItems.Commited = int.Parse(_reader.GetString(5).ToString());
                        valoresItems.exists = int.Parse(_reader.GetString(4).ToString()) - (int.Parse(_reader.GetString(3).ToString()) + int.Parse(_reader.GetString(5).ToString()));

                       
                        valoresItems = new Items();
                    }
                }
                _reader.Close();
                //cnn.Close();

                return listaItems;
            }
        }
        public static List<Items> findAllItemsSAP()
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("select T0.\"ItemCode\",T0.\"ItemName\",T0.\"CodeBars\",T1.\"OnHand\",T1.\"IsCommited\",T1.\"OnOrder\",T1.\"OnHand\" -T1.\"IsCommited\" as \"Total\" from \"" + Properties.Settings.Default.CompanyDB + "\".oitm T0 ");
                sb.Append("inner join " + Properties.Settings.Default.CompanyDB + ".OITW T1 on T0.\"ItemCode\" = T1.\"ItemCode\" ");
                sb.Append("where T1.\"WhsCode\" = '"+Properties.Settings.Default.WhsCode+ "' and T0.\"ItemType\" = 'I' and (T1.\"OnHand\" != null or T1.\"OnHand\" != 0)");

                using (HanaCommand command = new HanaCommand(sb.ToString(), cnn))
                {
                    HanaDataReader _reader = command.ExecuteReader();

                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            valoresItems.ItemCode = _reader.GetString(0).ToString();
                            valoresItems.ItemName = _reader.GetString(1).ToString();
                            valoresItems.SKU = _reader.GetString(2).ToString();
                            //valoresItems.Quantity = int.Parse(_reader.GetString(3).ToString());
                            valoresItems.OnHand = int.Parse(_reader.GetString(3).ToString().Split(',')[0]);
                            valoresItems.Commited = int.Parse(_reader.GetString(4).ToString().Split(',')[0]);
                            valoresItems.exists = int.Parse(_reader.GetString(3).ToString().Split(',')[0]) - (int.Parse(_reader.GetString(4).ToString().Split(',')[0]));

                            listaItems.Add(valoresItems);
                            valoresItems = new Items();
                        }
                    }
                    _reader.Close();
                    //cnn.Close();
                }
            }
            catch (Exception ex)
            {
               DL.Functions.Log(ex.Message);
            }
            return listaItems;
        }
        public static void getLastRegisterOINV(ref string docnum, ref string docentry, ref string id)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            using (HanaCommand command = new HanaCommand("select \"DOCNUM\",\"DOCENTRY\",\"ID\" from \"SYNCDB\".\"INVOICE\" order by \"ID\" desc limit 1", cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        docnum = _reader.GetString(0).ToString();
                        docentry = _reader.GetString(1).ToString();
                        id = _reader.GetString(2).ToString();
                    }
                }
                _reader.Close();
                //cnn.Close();
            }
        }
        public static void getLastRegisterGoodsEntrys(ref string docnum, ref string docentry, ref string id)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            using (HanaCommand command = new HanaCommand("select \"DOCNUM\",\"DOCENTRY\",\"ID\" from \"SYNCDB\".\"GOODS_ENTRYS\" order by \"ID\" desc limit 1", cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        docnum = _reader.GetString(0).ToString();
                        docentry = _reader.GetString(1).ToString();
                        id = _reader.GetString(2).ToString();
                    }
                }
                _reader.Close();
                //cnn.Close();
            }
        }
        public static void SyncOnHandSAPShopify(List<listas.Items> lista)
        {
            try
            {
                string query = string.Empty;

                foreach (var item in lista)
                {

                    //string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.ProductVariant + "/" +
                    //    item.idShopify + ".json";
                    query = "{\"location_id\": " + Properties.Settings.Default.location_id + "," +
                            "\"inventory_item_id\": " + item.inventory_item_id + "," +
                            "\"available_adjustment\": -" + item.Quantity + "}";

                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                        "/" + Properties.Settings.Default.adjunst;
                    //    item.idShopify + ".json";
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            request.Content = content;
                            HttpResponseMessage response = httpClient.SendAsync(request).Result;

                            JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            //JArray arr = (JArray)jsonObject["customer"];
                            //var code = jsonObject["code"].ToString();

                            if (response.IsSuccessStatusCode)
                            {
                                Log("Ajuste de stock por parte de las facturas listo sku: " + item.SKU);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static void SyncOnHandSAPShopifyPlus(List<listas.Items> lista)
        {
            try
            {
                string query = string.Empty;

                foreach (var item in lista)
                {

                    //string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.ProductVariant + "/" +
                    //    item.idShopify + ".json";
                    query = "{\"location_id\": " + Properties.Settings.Default.location_id + "," +
                            "\"inventory_item_id\": " + item.inventory_item_id + "," +
                            "\"available_adjustment\": " + item.Quantity + "}";

                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                        "/" + Properties.Settings.Default.adjunst;
                    //    item.idShopify + ".json";
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            request.Content = content;
                            HttpResponseMessage response = httpClient.SendAsync(request).Result;

                            JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            //JArray arr = (JArray)jsonObject["customer"];
                            //var code = jsonObject["code"].ToString();

                            if (response.IsSuccessStatusCode)
                            {
                                Log("Ajuste de stock por parte de las Entradas de mercancia listo sku: " + item.SKU);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static void SyncOnHandSAPShopifyPlusTransfer(List<listas.Items> lista)
        {
            try
            {
                string query = string.Empty;

                foreach (var item in lista)
                {

                    //string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.ProductVariant + "/" +
                    //    item.idShopify + ".json";
                    if (item.WhsCode == Settings.Default.WhsCode)
                    {
                        query = "{\"location_id\": " + Properties.Settings.Default.location_id + "," +
                          "\"inventory_item_id\": " + item.inventory_item_id + "," +
                          "\"available_adjustment\": " + item.Quantity + "}";
                    }
                    else
                    {
                        query = "{\"location_id\": " + Properties.Settings.Default.location_id + "," +
                          "\"inventory_item_id\": " + item.inventory_item_id + "," +
                          "\"available_adjustment\": -" + item.Quantity + "}";
                    }
                  

                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                        "/" + Properties.Settings.Default.adjunst;
                    //    item.idShopify + ".json";
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            request.Content = content;
                            HttpResponseMessage response = httpClient.SendAsync(request).Result;

                            JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            //JArray arr = (JArray)jsonObject["customer"];
                            //var code = jsonObject["code"].ToString();

                            if (response.IsSuccessStatusCode)
                            {
                                Log("Ajuste de stock por parte de las Transferencia listo sku: " + item.SKU);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static void SyncOnHandSAPShopify_old(List<listas.Items> lista)
        {
            try
            {
                foreach (var item in lista)
                {

                    //string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.ProductVariant + "/" +
                    //    item.idShopify + ".json";
                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                        "/" + Properties.Settings.Default.adjunst;
                    //    item.idShopify + ".json";
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage response = httpClient.SendAsync(request).Result;

                            JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            JArray arr = (JArray)jsonObject["customer"];
                            var code = jsonObject["code"].ToString();

                            foreach (var items in jsonObject["customer"])
                            {



                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static async void FindItemsBySKUShopify(List<listas.Items> lista)
        {
            try
            {
                for (global::System.Int32 i = 0; i < lista.Count; i++)
                {

                    string query = string.Empty;

                    //query = @"query {products(first:1, query:""sku:" + lista[i].SKU + "\") {\r\n edges\r\n        {\r\n node {\r\n id\r\n          }\r\n        }\r\n      }\r\n}";
                        query = @"
                                    {
                          products(first:1,query: " + (char)34 + lista[i].SKU + (char)34 + ") {" +
                       " edges {" +
                                       " node {" +

                                           " variants(first: 1) {" +
                                               " edges {" +
                                                   " node {" +
                                                       " inventoryQuantity" +
                                                       " inventoryItem{" +
                                                           " legacyResourceId" +
                                                           "}"+
                                                   " }" +
                                               " }" +
                                           " }" +
                                       " }" +
                                   " }" +
                               " }" +
                           " }";
                    var querys = new GraphQLRequest
                    {
                        Query = query,
                    };
                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/";

                    using (var httpClient = new HttpClient())
                    {

                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        var response = httpClient.PostAsJsonAsync("graphql.json", querys).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                          
                            JObject jsonObject = JObject.Parse(jsonResponse);

                            var valores = (dynamic)jsonObject["data"]["products"]["edges"].ToString();
                            lista[i].available_adjustment = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryQuantity"].ToString();
                            lista[i].inventory_item_id = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryItem"]["legacyResourceId"].ToString();
                            Globals.inventory_quantity = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryQuantity"].ToString();
                            Globals.inventory_item_id = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryItem"]["legacyResourceId"].ToString();
                        }

                        //using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        //{
                        //    httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        //    request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //    var response = httpClient.PostAsync(url, content).Result;

                        //    JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        //    JArray arr = (JArray)jsonObject["customer"];
                        //    var code = jsonObject["code"].ToString();

                        //    foreach (var item in jsonObject["customer"])
                        //    {



                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static string FindItemsByShopify(List<Products> listProduct)
        {
            string resultado = string.Empty;

            try
            {
                for (int i = 0; i < listProduct.Count; i++)
                {

                    string query = string.Empty;

                    //query = @"query {products(first:1, query:""sku:" + lista[i].SKU + "\") {\r\n edges\r\n        {\r\n node {\r\n id\r\n
                    //}\r\n        }\r\n      }\r\n}";
                    query = @"
                                    {
                          products(first:1,query: " + (char)34 + listaProduct[i].product.variants.sku + (char)34 + ") {" +
                   " edges {" +
                                   " node {" +

                                       " variants(first: 1) {" +
                                           " edges {" +
                                               " node {" +
                                                   " inventoryQuantity" +
                                                   " inventoryItem{" +
                                                       " legacyResourceId" +
                                                       "}" +
                                               " }" +
                                           " }" +
                                       " }" +
                                   " }" +
                               " }" +
                           " }" +
                       " }";
                    var querys = new GraphQLRequest
                    {
                        Query = query,
                    };
                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/";

                    using (var httpClient = new HttpClient())
                    {

                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        var response = httpClient.PostAsJsonAsync("graphql.json", querys).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = response.Content.ReadAsStringAsync().Result;

                            JObject jsonObject = JObject.Parse(jsonResponse);

                            var valores = (dynamic)jsonObject["data"]["products"]["edges"].ToString();
                            Globals.inventory_quantity = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryQuantity"].ToString();
                            Globals.inventory_item_id = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryItem"]["legacyResourceId"].ToString();
                        }

                    }
                }

                resultado = "";
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            return resultado;
        }
        public static string findInventoryItemIdandStock(string sku)
        {
            string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                       "/" + Properties.Settings.Default.adjunst;
            //    item.idShopify + ".json";
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                {
                    //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                    request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;

                    JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    JArray arr = (JArray)jsonObject["customer"];
                    var code = jsonObject["code"].ToString();

                    foreach (var items in jsonObject["customer"])
                    {



                    }
                }
            }
            return "";
        }
        public static string getMetaFieldsByIdCustomerShopify(string id)
        {
            string id_customer_sap = string.Empty;
            try
            {
                //var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                     Properties.Settings.Default.customerShopify + id + Settings.Default.metafields;
                //    item.idShopify + ".json";
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //request.Content = content;
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        //JArray arr = (JArray)jsonObject["customer"];
                        //var code = jsonObject["code"].ToString();
                        JArray arr = (JArray)jsonObject["metafields"];
                        if (response.IsSuccessStatusCode)
                        {
                            //for (global::System.Int32 i = 0; i < jsonObject["metafields"].ToArray()[1].Count; i++)
                            //{

                            //}
                            id_customer_sap = (string)((JProperty)arr[0].ToArray()[3]).Value;
                            //id_customer_sap = (dynamic)(jsonObject["metafields"].ToArray()[1]).ToArray()[3].GetType().GetProperties().GetValue(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log(ex.InnerException.InnerException.Message);
            }
            return id_customer_sap;
        }
        public static string ItemCodeSAPBySKUShopify(string sku)
        {
            string ItemCode = string.Empty;
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                    cnn.Open();
                }

                sQuery = "select \"ItemCode\" from \""+ Settings.Default.CompanyDB +"\".OITM where \"CodeBars\" = '" + sku + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ItemCode = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
            return ItemCode;
        }
        public static string ItemNameSAPBySKUShopify(string sku)
        {
            string ItemName = string.Empty;
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                    cnn.Open();
                }

                sQuery = "select \"ItemName\" from \"" + Settings.Default.CompanyDB + "\".OITM where \"CodeBars\" = '" + sku + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ItemName = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
            return ItemName;
        }
        public static string getCardNameBysCardCode(string cardCode)
        {
            string CardName = string.Empty;
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                    cnn.Open();
                }

                sQuery = "select \"CardName\" from \"" + Settings.Default.CompanyDB + "\".OCRD where \"CardCode\" = '" + cardCode + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    CardName = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
            return CardName;
        }
        public static string getPeyMethodCustomerSAP(string cardCode)
        {
            string CardName = string.Empty;
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                    cnn.Open();
                }

                sQuery = "select \"PymCode\" from \"" + Settings.Default.CompanyDB + "\".crd2 where \"CardCode\" = '" + cardCode + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    CardName = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
            return CardName;
        }
        public static void updateRegisterGoodsEntry(string id)
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }
                using (HanaCommand command = new HanaCommand("update \"SYNCDB\".\"PURCHASEORDERS\" SET \"SINCRONIZADO\" = '1' WHERE  \"ID\" ='" + id + "' ", cnn))
                {
                    command.ExecuteNonQuery();
                }
                //cnn.Close();
            }
            catch (HanaException ex)
            {
                Log("Error Hana error: " + ex.Message);
            }
        }
        public static void getLastRegisterInventoryEntrys(ref string docnum, ref string docentry, ref string id)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            using (HanaCommand command = new HanaCommand("select \"DOCNUM\",\"DOCENTRY\",\"ID\" from \"SYNCDB\".\"TRANSFERS\" where \"SYNC\" = '00' order by \"ID\" desc limit 1", cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        docnum = _reader.GetString(0).ToString();
                        docentry = _reader.GetString(1).ToString();
                        id = _reader.GetString(2).ToString();
                    }
                }
                _reader.Close();
                //cnn.Close();
            }
        }
        public static List<listas.Items> RecalcInventoryTransfer(string docnum)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("select T1.\"ItemCode\",T1.\"Dscription\",T1.\"CodeBars\",cast(T1.\"Quantity\" as int),cast(T2.\"OnHand\" as int)," +
                "cast(T2.\"IsCommited\" as int), T0.\"WhsCode\" from \"" + Properties.Settings.Default.CompanyDB + "\".OWTR T0 ");
            sb.Append("inner join " + Properties.Settings.Default.CompanyDB + ".WTR1 T1 on T0.\"DocEntry\" = T1.\"DocEntry\" ");
            sb.Append("Inner join " + Properties.Settings.Default.CompanyDB + ".OITW T2 on T1.\"ItemCode\" = T2.\"ItemCode\" ");
            sb.Append("where T0.\"DocNum\" = '" + docnum + "' and T1.\"WhsCode\" = '"+ Properties.Settings.Default.WhsCode +"'");

            using (HanaCommand command = new HanaCommand(sb.ToString(), cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        //if (_reader.GetString(6).ToString() == Properties.Settings.Default.WhsCode)
                        //{
                            valoresItems.ItemCode = _reader.GetString(0).ToString();
                            valoresItems.ItemName = _reader.GetString(1).ToString();
                            valoresItems.SKU = _reader.GetString(2).ToString();
                            valoresItems.Quantity = int.Parse(_reader.GetString(3).ToString());
                            valoresItems.OnHand = int.Parse(_reader.GetString(4).ToString());
                            valoresItems.Commited = int.Parse(_reader.GetString(5).ToString());
                            valoresItems.exists = int.Parse(_reader.GetString(4).ToString()) - (int.Parse(_reader.GetString(3).ToString()) + int.Parse(_reader.GetString(5).ToString()));
                            valoresItems.WhsCode = _reader.GetString(6).ToString();

                            listaItems.Add(valoresItems);
                            valoresItems = new Items();
                        //}
                    }
                }
                _reader.Close();
                //cnn.Close();

                return listaItems;
            }
        }
        public static List<listas.Items> RecalcGoodsIssue(string docnum)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }
            if (string.IsNullOrEmpty(docnum))
            {
                return listaItems;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("select T1.\"ItemCode\",T1.\"Dscription\",T1.\"CodeBars\",cast(T1.\"Quantity\" as int),cast(T2.\"OnHand\" as int)," +
                "cast(T2.\"IsCommited\" as int), T0.\"WhsCode\" from \"" + Properties.Settings.Default.CompanyDB + "\".OIGE T0 ");
            sb.Append("inner join " + Properties.Settings.Default.CompanyDB + ".IGE1 T1 on T0.\"DocEntry\" = T1.\"DocEntry\" ");
            sb.Append("Inner join " + Properties.Settings.Default.CompanyDB + ".OITW T2 on T1.\"ItemCode\" = T2.\"ItemCode\" ");
            sb.Append("where T0.\"DocNum\" = '" + docnum + "' and T1.\"WhsCode\" = '" + Properties.Settings.Default.WhsCode + "'");

            using (HanaCommand command = new HanaCommand(sb.ToString(), cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        //if (_reader.GetString(6).ToString() == Properties.Settings.Default.WhsCode)
                        //{
                        valoresItems.ItemCode = _reader.GetString(0).ToString();
                        valoresItems.ItemName = _reader.GetString(1).ToString();
                        valoresItems.SKU = _reader.GetString(2).ToString();
                        valoresItems.Quantity = int.Parse(_reader.GetString(3).ToString());
                        valoresItems.OnHand = int.Parse(_reader.GetString(4).ToString());
                        valoresItems.Commited = int.Parse(_reader.GetString(5).ToString());
                        valoresItems.exists = int.Parse(_reader.GetString(4).ToString()) - (int.Parse(_reader.GetString(3).ToString()) + int.Parse(_reader.GetString(5).ToString()));
                        valoresItems.WhsCode = _reader.GetString(6).ToString();

                        listaItems.Add(valoresItems);
                        valoresItems = new Items();
                        //}
                    }
                }
                _reader.Close();
                //cnn.Close();

                return listaItems;
            }
        }
        public static Products GetPricesSAP(Products lista)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }

            using (HanaCommand command = new HanaCommand("select \"Price\" from \""+ Properties.Settings.Default.CompanyDB +"\".\"ITM1\" where \"ItemCode\" = '"+ product.product.variants.sku +"' and \"PriceList\" = '3';", cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        docnum = _reader.GetString(0).ToString();
                        docentry = _reader.GetString(1).ToString();
                        //id = _reader.GetString(2).ToString();
                    }
                }
                _reader.Close();
                //cnn.Close();
            }
            return product;
        }
        public static List<Products> getAllProductPrices()
        {
            //string query = "SELECT * FROM \"" + Properties.Settings.Default.CompanyDB + "\".\"PRICES\" WHERE \"SINC\" ='00'";
            string query = "SELECT * FROM \"SYNCDB\".\"PRICES\" WHERE \"SINC\" ='00'";
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }
            listaProductSAP = new List<ProductSAP>();

            using (HanaCommand command = new HanaCommand(query, cnn))
            {
                HanaDataReader _reader = command.ExecuteReader();

                if (_reader.HasRows)
                {
                    while (_reader.Read())
                    {
                        productSAP.Id = _reader.GetString(0).ToString();
                        productSAP.ItemCode = _reader.GetString(1).ToString();
                        productSAP.ItemName = _reader.GetString(2).ToString();
                        //id = _reader.GetString(2).ToString();
                        listaProductSAP.Add(productSAP);
                        productSAP = new ProductSAP();
                    }
                }
                _reader.Close();
                //cnn.Close();
            }

            listaProduct = new List<Products>();

            for (int i = 0; i < listaProductSAP.Count; i++)
            {
                query = "select T0.\"ItemCode\",T0.\"ItemName\",T0.\"CodeBars\",T1.\"Price\"" +
                        "from \"" + Properties.Settings.Default.CompanyDB + "\".ITM1 T0 " +
                        "inner join \"" + Properties.Settings.Default.CompanyDB + "\".OITM T1 on T0.\"ItemCode\" = T1.\"ItemCode\" " +
                        "where T1.\"ItemCode\" = '" + listaProductSAP[i].ItemCode + "' and T0.\"PriceList\" = '"+ Settings.Default.ListPrices    +"' ";

                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }

                using (HanaCommand command = new HanaCommand(query, cnn))
                {
                    HanaDataReader _reader = command.ExecuteReader();

                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            product.product.id = listaProductSAP[i].Id;
                            product.product.title = _reader.GetString(1).ToString();
                            product.product.variants.sku = _reader.GetString(2).ToString();
                            product.product.variants.price = _reader.GetString(3).ToString();
                            listaProduct.Add(product);
                            product.product = new product();
                        }
                        _reader.Close();
                        //cnn.Close();
                    }
                }
            }
         
            return listaProduct;
        }
        public static async void SynPricesShopify(List<Products> lista)
        {
            try
            {
                for (global::System.Int32 i = 0; i < lista.Count; i++)
                {

                    string query = string.Empty;

                    //query = @"query {products(first:1, query:""sku:" + lista[i].SKU + "\") {\r\n edges\r\n        {\r\n node {\r\n id\r\n          }\r\n        }\r\n      }\r\n}";
                    query = @"
                                    {
                          products(first:1,query: " + (char)34 + lista[i].product.variants.sku + (char)34 + ") {" +
                                 " edges {" +
                                   " node {" +
                                   "id title handle " +
                                   //" variants(first: 1) {" +
                                   //    " edges {" +
                                   //        " node {" +
                                   //            " inventoryQuantity" +
                                   //            " inventoryItem{" +
                                   //                " legacyResourceId" +
                                   //                "}" +
                                   //        " }" +
                                   //    " }" +
                                   //" }" +
                                   " }" +
                               " }" +
                           " }" +
                       " }";
                    var querys = new GraphQLRequest
                    {
                        Query = query,
                    };
                    var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/";

                    using (var httpClient = new HttpClient())
                    {

                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        var response = httpClient.PostAsJsonAsync("graphql.json", querys).Result;

                        if (response.IsSuccessStatusCode)
                        {   
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            JObject jsonObject = JObject.Parse(jsonResponse);

                            var valores = (dynamic)jsonObject["data"]["products"]["edges"].ToString();
                            lista[i].product.id = jsonObject["data"]["products"]["edges"][0]["node"]["id"].ToString().Replace("gid://shopify/Product/","");
                            //lista[i].available_adjustment = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryQuantity"].ToString();
                            //lista[i].inventory_item_id = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryItem"]["legacyResourceId"].ToString();
                            //Globals.inventory_quantity = jsonObject["data"]["products"]["edges"][0]["node"]["variants"]["edges"][0]["node"]["inventoryQuantity"].ToString();
                            if(!updateVariantsShopify(lista[i].product.id,
                                                   lista[i].product.variants.sku,
                                                   lista[i].product.variants.price, ""))
                            {
                                Log("No se puede actualizar el precio del siguiente producto: " + lista[i].product.variants.sku);
                            }
                            else
                            {
                                Log("Se actualizo el precio del siguiente producto: " + lista[i].product.variants.sku);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
        public static async void CreateNewProduct(List<Products> Lista)
        {
            int i = 0;

            try
            {
                foreach (Products product in Lista)
                {
                    i++;
                    string query = string.Empty;
                    string product_id = string.Empty;
                    string inventory_item_id = string.Empty;

                    var json = JsonConvert.SerializeObject(product);
                    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                    //query = "{\"product\": {" +
                    //    "\"title\": \"" + product.product.title + "\"," +
                    //    "\"variants\": {" +
                    //        "\"price\": \"" + product.product.variants.price + "\"," +
                    //        "\"sku\": \"" + product.product.variants.sku + "\"" +
                    //        "}" +
                    //        "}" +
                    //     "}";

                    // var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                    if (i/320 == 1)
                    {
                        Thread.Sleep(5000);
                    }
                    //Thread.Sleep(1000);
                    if (ValidIfExitsItem(product.product.variants.sku))
                    {

                        string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" +
                            Properties.Settings.Default.Product;
                        try
                        {
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                                {
                                    request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    request.Content = content;
                                    HttpResponseMessage response = httpClient.SendAsync(request).Result;

                                    try
                                    {

                                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                                        var a = (dynamic)jsonObject["product"];
                                        //foreach (var item in jsonObject)
                                        //{
                                        //    var a = (dynamic)item.Value[1];
                                        //    var uno = item.ToString();
                                        //}
                                        product.product.id = a["id"];
                                        foreach (var item in a["variants"])
                                        {
                                            product.product.variants.product_id = item["product_id"];
                                            product_id = item["id"];
                                            inventory_item_id = item["inventory_item_id"];
                                        }
                                        Thread.Sleep(800);
                                        if (updateVariantsShopify(product_id, product.product.variants.sku, product.product.variants.price,
                                             product.product.variants.inventory_quantity))
                                        {
                                            Thread.Sleep(900);
                                            if (updateTackeed(inventory_item_id))
                                            {
                                                Thread.Sleep(1000);
                                                if (!updateStock(inventory_item_id, product.product.variants.inventory_quantity))
                                                {
                                                    Log("Error: No se pudo actualizar el stock del articulo" +
                                                        "artículo");
                                                }
                                            }
                                            else
                                            {
                                                Log("Error: No se pudo actualizar el seguimiento del artículo");
                                            }
                                        }
                                        else
                                        {
                                            Log("Error: No se pudo actualizar las variantes del artículo");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Thread.Sleep(4000);
                                        Log(ex.Message + " Error: " + product.product.variants.sku);
                                        ///throw; 
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(3000);
                            Log(ex.ToString()); 
                            Log( i +" Error al querer crear el siguiente produto en shopify: " + product.product.variants.sku);
                        }
                    }
                }
                Log("Se termino de sincronizar los artículos");
            }
            catch (Exception ex)
            {
                Log(i + " " + ex.Message);
            }
        }
        public static bool updateTackeed(string id)
        {
            bool band = false;
            try
            {
                string query = string.Empty;

                query = "{\"inventory_item\":{\"id\":" + id + ",\"tracked\":true}}";

                var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                //string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" +
                //            Properties.Settings.Default.ProductVariant;
                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/2023-10/inventory_items/" +
                        id + ".json";

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("PUT"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Content = content;
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        var a = (dynamic)jsonObject["variants"];

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            band = true;
                        }
                        else
                        {
                            band = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Erro: " + ex.Message);
            }
            return band;
        }
        public static bool updateVariantsShopify (string id, string sku,string price,string stock)
        {
            bool band = false;
            try
            {
                string query = string.Empty;
                //var json = JsonConvert.SerializeObject(product);
                //var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                query = "{\"variant\": {" +
                        "\"id\": \"" + id + "\"," +
                            "\"sku\": \"" + sku + "\"," +
                            "\"price\": \"" + price.Replace(",",".") + "\"" +
                            "}" +
                         "}";

                 var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                //string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" +
                //            Properties.Settings.Default.ProductVariant;
                string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.ProductVariant + "/" +
                        id + ".json";

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("PUT"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Content = content;
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        var a = (dynamic)jsonObject["variants"];

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            band = true;
                        }
                        else
                        {
                            band = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log(ex.Message);
            }
            return band;
        }
        public static bool updateStock(string item_id, string stock)
        {
            bool band = false;
            try
            {
                string query = string.Empty;
                //var json = JsonConvert.SerializeObject(product);
                //var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                query = "{\"location_id\": "+Settings.Default.location_id+ ",\"inventory_item_id\":"+ item_id + ",\"available_adjustment\":" + stock.Split(',')[0] +"}";

                var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                //string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" +
                //            Properties.Settings.Default.ProductVariant;
                string url = "https://" + Properties.Settings.Default.dominio + Properties.Settings.Default.inventory_levels;

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Content = content;
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        //var a = (dynamic)jsonObject["variants"];
                       
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            band = true;
                        }
                        else
                        {
                            band = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log(ex.Message);
            }
            return band;
        }
        public static bool ValidIfExitsItem(string sku)
        {
            bool band = false;
            try
            {
                string query = string.Empty;

                //query = @"query {products(first:1, query:""sku:" + lista[i].SKU + "\") {\r\n edges\r\n        {\r\n node {\r\n id\r\n
                //}\r\n        }\r\n      }\r\n}";
                query = @"
                           query {productVariants(first: 3, query: ""sku:" +sku +"\") {" +
                           "edges   { " +
                                "node       { "+
                                     "id " +
                                        "}" +
                                    "}" +
                                "}" +
                           " }";

                //query = @"query { " +
                //          "product(id: \"gid://shopify/Product/"+ id +"\") { " +
                //          " title " +
                //          " totalInventory " +
                //          "} " +
                //       " }";


                var querys = new GraphQLRequest
                {
                    Query = query,
                };
                var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/";

                using (var httpClient = new HttpClient())
                {

                    httpClient.BaseAddress = new Uri(url);
                    httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                    var response = httpClient.PostAsJsonAsync("graphql.json", querys).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = response.Content.ReadAsStringAsync().Result;

                        JObject jsonObject = JObject.Parse(jsonResponse);
                        var jarray = jsonObject["data"]["productVariants"]["edges"].Count();
                        //band = true;
                        if (jarray == 0)
                        {
                            band = true;
                        }
                        else
                        {
                            band = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                band= false;
                Log(ex.Message + " - Error: " +ex.InnerException.InnerException.Message);
            }
            return band;
        }
        public static List<Products> CompleteListProductFromShopify()
        {
            try
            {
                string query = string.Empty;
                product.product = new product();
                product.product.variants = new variants();
                listaProduct = new List<Products>();
                System.Globalization.CultureInfo culInfo = new System.Globalization.CultureInfo("es-Mx");

                query = "select distinct T0.\"ItemCode\",T1.\"ItemName\",T1.\"CodeBars\",cast(T0.\"Price\" as decimal(19,2)),(T1.\"OnHand\" - T1.\"IsCommited\") as \"Disponible\", T1.\"FrgnName\" " +
                        "from \"" + Properties.Settings.Default.CompanyDB + "\".ITM1 T0 " +
                        "inner join \"" + Properties.Settings.Default.CompanyDB + "\".OITM T1 on T0.\"ItemCode\" = T1.\"ItemCode\" " +
                        "where T1.\"ItemType\" = 'I' and T1.\"CodeBars\"!= '' and T0.\"PriceList\" = '"+ Settings.Default.ListPrices +"' ";

                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }

                using (HanaCommand command = new HanaCommand(query, cnn))
                {
                    HanaDataReader _reader = command.ExecuteReader();

                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            product.product.title = _reader.GetString(5).ToString();
                            product.product.variants.sku = _reader.GetString(2).ToString();
                            product.product.variants.price = _reader.GetString(3).ToString();
                            product.product.variants.inventory_quantity = _reader.GetString(4).ToString();
                            //product.product.body_html = _reader.GetString(5).ToString();
                            listaProduct.Add(product);
                            product = new Products();
                            product.product = new product();
                            product.product.variants = new variants();
                        }
                        _reader.Close();
                        //cnn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            return listaProduct;
        }
        public static List<customers> getListCustomerShopify()
        {
            try
            {
                listCustomer = new List<customers>();
                customers customers = new customers();
                addresses addresses = new addresses();
                customers.addresses = new List<addresses>();
                customers.default_Address = new List<default_address>();
                default_address default_Address = new default_address();

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                 Properties.Settings.Default.api_v_shopify + "/customers.json";

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        JArray arr = (JArray)jsonObject["customers"];

                        foreach (var item in jsonObject["customers"])
                        {
                            customers.id = item["id"].ToString();
                            customers.email = item["email"].ToString();
                            customers.first_name = item["first_name"].ToString();
                            customers.last_name = item["last_name"].ToString();
                            customers.currency = item["currency"].ToString();
                            customers.phone = item["phone"].ToString();
                            foreach (var items in item["addresses"])
                            {

                                addresses.id = items["id"].ToString();
                                addresses.customer_id = items["customer_id"].ToString();
                                addresses.first_name = items["first_name"].ToString();
                                addresses.last_name = items["last_name"].ToString();
                                addresses.company = items["company"].ToString();
                                addresses.address1 = items["address1"].ToString();
                                addresses.address2 = items["address2"].ToString();
                                addresses.city = items["city"].ToString();
                                addresses.province = items["province"].ToString();
                                addresses.country = items["country"].ToString();
                                addresses.zip = items["zip"].ToString();
                                addresses.phone = items["phone"].ToString();
                                addresses.name = items["name"].ToString();
                                addresses.province_code = items["province_code"].ToString();
                                addresses.country_code = items["country_code"].ToString();
                                addresses.country_name = items["country_name"].ToString();
                            }
                            
                            for (global::System.Int32 i = 0; i < item["default_address"].Count(); i++)
                            {

                                //default_Address.id = ()item["default_address"][0].ToString();
                                //default_Address.customer_id = items2["customer_id"].ToString();
                                //default_Address.first_name = items2["first_name"].ToString();
                                //default_Address.last_name = items2["last_name"].ToString();
                                //default_Address.company = items2["company"].ToString();
                                //default_Address.address1 = items2["address1"].ToString();
                                //default_Address.address2 = items2["address2"].ToString();
                                //default_Address.city = items2["city"].ToString();
                                //default_Address.province = items2["province"].ToString();
                                //default_Address.country = items2["country"].ToString();
                                //default_Address.zip = items2["zip"].ToString();
                                //default_Address.phone = items2["phone"].ToString();
                                //default_Address.name = items2["name"].ToString();
                                //default_Address.province_code = items2["province_code"].ToString();
                                //default_Address.country_code = items2["country_code"].ToString();
                                //default_Address.country_name = items2["country_name"].ToString();
                            }

                            customers.addresses.Add(addresses);
                            customers.default_Address.Add(default_Address);

                            listCustomer.Add(customers);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log("Error:" + ex.Message);
            }
            return listCustomer;
        }
        public static void GetCustomerSAP()
        {
            try
            {
                string query = "SELECT \"CardCode\",\"CardName\",\"E_Mail\" FROM \"" + Properties.Settings.Default.CompanyDB + "\".\"OCRD\" WHERE \"CardType\" ='C' order by \"CardCode\" asc ";
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }
                customersSAP values = new customersSAP();
                
                using (HanaCommand command = new HanaCommand(query, cnn))
                {
                    HanaDataReader _reader = command.ExecuteReader();

                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            values.CardCode = _reader.GetString(0).ToString();
                            values.CardName = _reader.GetString(1).ToString();
                            values.email = _reader.GetString(2).ToString();

                            FindCustomerShopifyByEmail(values.email,values.CardCode, values.CardName);
                            values = new customersSAP();
                        }
                    }
                    _reader.Close();
                    //cnn.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error en GetCustomerSAP: " + ex.Message);
            }
        }
        public static void FindCustomerShopifyByEmail(string email, string cardCode, string cardname)
        {
            customers customers = new customers();
            try
            {
                listCustomer = new List<customers>();
                addresses addresses = new addresses();
                customers.addresses = new List<addresses>();
                customers.default_Address = new List<default_address>();
                default_address default_Address = new default_address();

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                 Properties.Settings.Default.api_v_shopify + "/customers/search.json?query=email:" + email;

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        Thread.Sleep(1000);
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        JArray arr = (JArray)jsonObject["customers"];
                        if (arr.Count == 0 || arr == null )
                        {
                            BL.Functions.CreateCustomer(cardname.Split(' ')[0], cardname.Split(' ')[1] != null ? "": cardname.Split(' ')[1], email, "",cardCode); ;

                            return;
                        }

                        foreach (var item in jsonObject["customers"])
                        {
                            GetMetafieldsFromCustomerById(item["id"].ToString(), cardCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error en FindCustomerShopifyByEmail: " + ex.Message);
            }
            //return customers;
        }
        public static void GetMetafieldsFromCustomerById(string Id, string cardCode)
        {
            try
            {
                List<metafields> listMetafield = new List<metafields>();
                metafields values = new metafields();

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                 Properties.Settings.Default.api_v_shopify +  Settings.Default.customerShopify + Id + Settings.Default.metafields;

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        JArray arr = (JArray)jsonObject["metafields"];
                        if (arr.Count == 0)
                        {
                            CreateMetafieldsCustomerShopify(Id,cardCode);
                            return;
                        }

                        foreach (var item in jsonObject["metafields"])
                        {
                            if (item["key"].ToString() == Settings.Default.metafieldSAP)
                            {
                                values.value = cardCode;
                                values.description = item["description"].ToString();
                                values.updated_at = item["updated_at"].ToString();
                                values.owner_resource = item["owner_resource"].ToString();
                                values.admin_graphql_api_id = item["admin_graphql_api_id"].ToString();
                                values.created_at = item["created_at"].ToString();
                                values.id = item["id"].ToString();
                                values.key = item["key"].ToString();
                                values.owner_id = item["owner_id"].ToString();
                                values.type = item["type"].ToString();

                                listMetafield.Add(values);
                                values = new metafields();
                            }
                            else
                            {
                                values.value = item["value"].ToString();
                                values.description = item["description"].ToString();
                                values.updated_at = item["updated_at"].ToString();
                                values.owner_resource = item["owner_resource"].ToString();
                                values.admin_graphql_api_id = item["admin_graphql_api_id"].ToString();
                                values.created_at = item["created_at"].ToString();
                                values.id = item["id"].ToString();
                                values.key = item["key"].ToString();
                                values.owner_id = item["owner_id"].ToString();
                                values.type = item["type"].ToString();

                                listMetafield.Add(values);
                                values = new metafields();
                            }
                        
                        }
                        UpdateCustomerMetafieldsShopify(Id,listMetafield,cardCode);

                    }
                }

                //return "";
            }
            catch (Exception ex)
            {
                Log("Error en GetMetafieldsFromCustomerById: " + ex.Message);
            }
        }
        public static void CreateMetafieldsCustomerShopify(string Id,string cardcode)
        {
            try
            {
                var data = "{ \"metafield\" :{\"namespace\": \"custom\", \"key\": \"" + Settings.Default.metafieldSAP + "\", \"value\": \"" + cardcode + "\",\"type\": \"single_line_text_field\"}}";

                var content = new StringContent(data,Encoding.UTF8,"application/json");

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                 Properties.Settings.Default.api_v_shopify + Settings.Default.customerShopify + Id + Settings.Default.metafields;

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                    {
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        
                        request.Content = content;

                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        //JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            Functions.UpdateSyncBP(cardcode);
                            Log("Se actualizo el " + Settings.Default.metafieldSAP + " " + cardcode);
                        }
                        else
                        {
                            Log("Hubo un error al actualizar el " + Settings.Default.metafieldSAP + " " + cardcode);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log ("Error en CreateMetafieldsCustomerShopify : " +ex.Message);
            }
        }
        public static void UpdateCustomerMetafieldsShopify(string idcustomer,List<metafields> customers,string cardcode)
        {
            try
            {
                for (global::System.Int32 i = 0; i < customers.Count; i++)
                {

                    var data = "{ \"metafield\" :{ \"id\": \"" + customers[i].id + "\", \"value\": \"" + customers[i].value + "\",\"type\": \"single_line_text_field\"}}";

                    var content = new StringContent(data, Encoding.UTF8, "application/json");

                    string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                     Properties.Settings.Default.api_v_shopify + Settings.Default.customerShopify + idcustomer + "/metafields/"+ customers[i].id +".json";

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("PUT"), url))
                        {
                            request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            request.Content = content;

                            HttpResponseMessage response = httpClient.SendAsync(request).Result;

                            //JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                            {
                                Components.DL.Functions.UpdateSyncBP(cardcode);
                                Log("Se actualizo el " + Settings.Default.metafieldSAP + " " + customers[i].value);

                            }
                            else
                            {
                                Log("Hubo un error al actualizar el " + Settings.Default.metafieldSAP + " " + customers[i].value);
                            }

                        }
                    }
                }
              
            }
            catch (Exception ex)
            {
                Log(ex.Message.ToString());
            }
           
        }
        public static string GetLastRegisterBP()
        {
                string CardCode,CardName,E_Mail = string.Empty;
            try
            {
                try
                {
                    if (cnn.State == System.Data.ConnectionState.Closed)
                    {
                        cnn = new HanaConnection(Settings.Default.HanaConection);
                        cnn.Open();
                    }

                    sQuery = "select \"CardCode\",\"CardName\" from \"SYNCDB\".\"BUSINESS_PARTNER\" where \"SYNC\" = '00'; ";
                    cmd = new HanaCommand(sQuery, cnn);
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CardCode = reader.GetString(0);
                        CardName = reader.GetString(1);
                        E_Mail = GetEmailBPfromCardCode(CardName);
                        FindCustomerShopifyByEmail(E_Mail, CardCode,CardName);
                    }
                    reader.Close();
                    //cnn.Close();
                }
                catch (Exception ex)
                {
                    DL.Functions.Log(ex.Message);
                }
                return E_Mail;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static string GetEmailBPfromCardCode(string cardcode)
        {
                string E_Mail = string.Empty;
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                {
                    cnn = new HanaConnection(Settings.Default.HanaConection);
                }

                sQuery = "select \"E_Mail\" from \"" + Settings.Default.CompanyDB + "\".OCRD where \"CardCode\" = '" + CardCode + "'";
                cmd = new HanaCommand(sQuery, cnn);
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    E_Mail = reader.GetString(0);
                }
                reader.Close();
                //cnn.Close();
}
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            return E_Mail;
        }
        public static void UpdateSyncBP(string CardCode)
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }
                using (HanaCommand command = new HanaCommand("update \"SYNCDB\".\"BUSINESS_PARTNER\" SET \"SINCRONIZADO\" = '1' WHERE  \"CARDCODE\" ='" + CardCode + "' ", cnn))
                {
                    command.ExecuteNonQuery();
                }
                //cnn.Close();
            }
            catch (HanaException ex)
            {
                Log("Error Hana error: " + ex.Message);
            }
        }
        public static void UpdateSyncPrices(string Id)
        {

        }
        //public static string UpdatePricesShopify()
        //{

        //}
        public static void Log(string error)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string hoy = DateTime.Today.ToShortDateString().Replace("/", "-");


                if (!Directory.Exists(path + "//Logs"))
                {
                    Directory.CreateDirectory(path + "//Logs");
                }
                if (!File.Exists(path + "//Logs//" + hoy + ".txt"))
                {
                    File.Exists(path + "//Logs//" + hoy + ".txt");
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "//Logs//" + hoy + ".txt", true))
                {
                    file.WriteLine(DateTime.Now + " - " + error);
                }

            }
            catch (Exception ex)
            {
                //Log(ex.Message);
            }
        }
    }
}