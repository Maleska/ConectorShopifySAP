using ConectorShopifySAP.Components.DL.listas;
using ConectorShopifySAP.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace ConectorShopifySAP.Components.BL
{
    internal class Functions
    {
        #region Variables

        public static List<DL.listas.OVShopify> listaOVShopify = new List<DL.listas.OVShopify>();
        public static DL.listas.OVShopify valoresShopify =  new DL.listas.OVShopify();
        public static List<DL.listas.detailOVShopify> listaDetalleShopidy = new List<DL.listas.detailOVShopify>();  
        public static DL.listas.detailOVShopify detailShopify = new DL.listas.detailOVShopify();
        public static discount_application discount_application = new discount_application();

        /// <SAP>
        private static int lastRowCountOINV, lastRowCountOrdr = 0;
        private static int lastRowCountGoodsEntrys;
        private static int lastRowCountInvenetoryTransfer , lastRowCountGoodsIssue, lastRowCountBP;
        private static string docnum, id, docentry;
        private static List<DL.listas.Items> ListaItems = new List<DL.listas.Items>();

        private static List<DL.listas.OVSAP> ListaSAP = new List<DL.listas.OVSAP>();
        private static DL.listas.OVSAP valoresSAP = new DL.listas.OVSAP();  
        private static DL.listas.OVSAPDetail vSAPDetail = new DL.listas.OVSAPDetail();
        private static List<DL.listas.OVSAPDetail> listaDetalleSAP = new List<DL.listas.OVSAPDetail>();
        private static OVSAP ovsap = new OVSAP();
        private static PagoFactura paymentInvoices = new PagoFactura();
        private static PagoFactura PagoFactura = new PagoFactura();
        private static List<PagoFactura> PaymentInvoicesList = new List<PagoFactura>(); 
        private static List<Products> ProductList = new List<Products>();
        public static List<Products> listaProduct;
        /// </summary>

        #endregion

        //Obtenemos los pedidos de shopify Todos
        public static void initShopify()
        {
            string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify + "/" + 
                Properties.Settings.Default.orderShopify;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                    request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = httpClient.SendAsync(request).Result;

                    JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    JArray arr = (JArray)jsonObject["orders"];
                    //var code = jsonObject["code"].ToString();

                    foreach (var item in jsonObject["orders"]) {

                        if (!Components.DL.Functions.getexistsOrders(item["order_number"].ToString(),"DermaExpress"))
                        {
                            valoresShopify.idShopify = item["id"].ToString();
                            valoresShopify.name = item["name"].ToString();
                            valoresShopify.order_number = item["order_number"].ToString();
                            valoresShopify.customer = (item["customer"].ToArray()).ToList()[0].Last().ToString();
                            valoresShopify.email = (item["customer"].ToArray()).ToList()[1].Last().ToString();
                            valoresShopify.firstname = (item["customer"].ToArray()).ToList()[5].Last().ToString();
                            valoresShopify.lastname = (item["customer"].ToArray()).ToList()[6].Last().ToString();
                            valoresShopify.customer_id = (item["customer"].ToArray()).ToList()[0].Last().ToString();
                            valoresShopify.created_at = item["created_at"].ToString();
                            valoresShopify.financial_status = item["financial_status"].ToString();
                            valoresShopify.payment_gateway_names = item["payment_gateway_names"].ToString();

                            valoresShopify.ditails = new List<DL.listas.detailOVShopify>();
                            valoresShopify.discount = new discount_application();

                            foreach (var lineitem in item["line_items"])
                            {
                                detailShopify.sku = lineitem["sku"].ToString();
                                detailShopify.quantity = lineitem["quantity"].ToString();
                                if (lineitem["price"].ToString() == "0.0")
                                {
                                    detailShopify.price = "0.01";
                                }
                                else
                                {
                                    detailShopify.price = lineitem["price"].ToString();
                                }
                                foreach (var itemtax in item["tax_lines"].ToArray())
                                {
                                    detailShopify.tax_line = itemtax["rate"].ToString();
                                }

                                detailShopify.dscription = lineitem["title"].ToString();
                                detailShopify.discount = lineitem["total_discount"].ToString();
                                valoresShopify.ditails.Add(detailShopify);
                            }
                            foreach (var discount in item["discount_applications"])
                            {
                                discount_application.allocation_method = discount["allocation_method"].ToString();
                                discount_application.value = discount["value"].ToString();
                                discount_application.value_type = discount["value_type"].ToString();
                                discount_application.code = discount["code"].ToString();
                                discount_application.target_selection = discount["target_selection"].ToString();
                                discount_application.target_type = discount["target_type"].ToString();
                                discount_application.type = discount["type"].ToString();
                            }
                            valoresShopify.discount = discount_application;
                            //valoresShopify.ditails.Add(listaDetalleShopidy);
                            listaOVShopify.Add(valoresShopify);
                        }

                    }
                }
            }
            ValidOVShopify(listaOVShopify);
        }
        public static void getOrderShopifyByDate(string date)
        {
            try
            {
                //formato de fecha debe de ser el siguiente 2023-09-11
                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" +
                    Properties.Settings.Default.api_v_shopify + "/" + Properties.Settings.Default.ordersShopifyByDate + date;
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                    {
                        //httpClient.DefaultRequestHeaders.Add(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        request.Headers.TryAddWithoutValidation(name: "X-Shopify-Access-Token", value: Properties.Settings.Default.TokenShopify);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage response = httpClient.SendAsync(request).Result;

                        JObject jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        JArray arr = (JArray)jsonObject["orders"];
                        //var code = jsonObject["code"].ToString();

                        foreach (var item in jsonObject["orders"])
                        {

                            if (!Components.DL.Functions.getexistsOrders(item["order_number"].ToString(), "DermaExpress"))
                            {
                                valoresShopify.idShopify = item["id"].ToString();
                                valoresShopify.name = item["name"].ToString();
                                valoresShopify.order_number = item["order_number"].ToString();
                                valoresShopify.customer = (item["customer"].ToArray()).ToList()[0].Last().ToString();
                                valoresShopify.email = (item["customer"].ToArray()).ToList()[1].Last().ToString();
                                valoresShopify.firstname = (item["customer"].ToArray()).ToList()[5].Last().ToString();
                                valoresShopify.lastname = (item["customer"].ToArray()).ToList()[6].Last().ToString();
                                valoresShopify.created_at = item["created_at"].ToString();
                                valoresShopify.customer_id = (item["customer"].ToArray()).ToList()[0].Last().ToString();
                                valoresShopify.payment_gateway_names = item["payment_gateway_names"].ToString();

                                valoresShopify.ditails = new List<DL.listas.detailOVShopify>();
                                valoresShopify.discount = new discount_application();

                                foreach (var lineitem in item["line_items"])
                                {
                                    detailShopify.sku = lineitem["sku"].ToString();
                                    detailShopify.quantity = lineitem["quantity"].ToString();
                                    detailShopify.price = lineitem["price"].ToString();

                                    foreach (var itemtax in item["tax_lines"].ToArray())
                                    {
                                        detailShopify.tax_line = itemtax["rate"].ToString();
                                    }

                                    detailShopify.dscription = lineitem["title"].ToString();
                                    detailShopify.discount = lineitem["total_discount"].ToString();
                                    valoresShopify.ditails.Add(detailShopify);
                                }

                                foreach (var discount in item["discount_applications"])
                                {
                                    discount_application.allocation_method = discount["allocation_method"].ToString();
                                    discount_application.value = discount["value"].ToString();
                                    discount_application.value_type = discount["value_type"].ToString();
                                    discount_application.code = discount["code"].ToString();
                                    discount_application.target_selection = discount["target_selection"].ToString();
                                    discount_application.target_type = discount["target_type"].ToString();
                                    discount_application.type = discount["type"].ToString();
                                }
                                valoresShopify.discount = discount_application;
                                //valoresShopify.ditails.Add(listaDetalleShopidy);
                                listaOVShopify.Add(valoresShopify);
                            }

                        }
                    }
                }
                ValidOVShopify(listaOVShopify);
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
        }
        public static void ValidNewRegisterOINV()
        {
            int currentRowCount = Convert.ToInt16(DL.Functions.getRowCountInvoice());

            if (currentRowCount > lastRowCountOINV)
            {
                lastRowCountOINV = currentRowCount;
                DL.Functions.getLastRegisterOINV(ref docnum, ref docentry,ref id);
                ListaItems = DL.Functions.RecalcInventoryInvoices(docnum);
                DL.Functions.FindItemsBySKUShopify(ListaItems);
                DL.Functions.SyncOnHandSAPShopify(ListaItems);
            }

        }
        public static void SyncAllItemsSapToShopify()
        {
            try
            {

                //ListaItems = DL.Functions.findAllItemsSAP();
                //DL.Functions.FindItemsBySKUShopify(ListaItems);
                //DL.Functions.SyncOnHandSAPShopify(ListaItems);
                listaProduct = DL.Functions.CompleteListProductFromShopify();
                DL.Functions.CreateNewProduct(listaProduct);
            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
        }
        public static void SyncPrices()
        {
            listaProduct = DL.Functions.getAllProductPrices();
            if (listaProduct.Count > 0)
            {

                Components.DL.Functions.SynPricesShopify(listaProduct);
            }
        }
        public static void ValidNewRegisterGoodsEntrys()
        {
            int currentRowCount = Convert.ToInt16(DL.Functions.getRowCountGoodsEntry());

            if (currentRowCount > lastRowCountGoodsEntrys )
            {
                lastRowCountGoodsEntrys = currentRowCount;

                DL.Functions.getLastRegisterGoodsEntrys(ref docnum, ref docentry, ref id);
                ListaItems = DL.Functions.RecalcInventoryGoodsEntry(docnum);
                if (ListaItems.Count > 0)
                {
                    DL.Functions.FindItemsBySKUShopify(ListaItems);
                    DL.Functions.SyncOnHandSAPShopifyPlus(ListaItems);
                }
            }
        }
        public static void ValidNewRegisterInventoryTransfer()
        {
            int currentRowCount = Convert.ToInt16(DL.Functions.getRowCountInventoryTransfer());
            if (currentRowCount > lastRowCountInvenetoryTransfer)
            {
                lastRowCountInvenetoryTransfer = currentRowCount;
                ListaItems = DL.Functions.RecalcInventoryTransfer(docnum);
                if (ListaItems.Count > 0)
                {
                    DL.Functions.FindItemsBySKUShopify(ListaItems);
                    DL.Functions.SyncOnHandSAPShopifyPlusTransfer(ListaItems);
                }

            }
        }
        public static void ValirNewRegisterGoodsIssue()
        {
            int currentRowCount = Convert.ToInt16(DL.Functions.getRowCountGoodsIssue());

            if (currentRowCount == lastRowCountGoodsIssue)
            {
                lastRowCountGoodsIssue = currentRowCount;
                ListaItems = DL.Functions.RecalcGoodsIssue(docnum);
                if (ListaItems.Count > 0)
                {
                    DL.Functions.FindItemsBySKUShopify(ListaItems);
                    DL.Functions.SyncOnHandSAPShopifyPlusTransfer(ListaItems);
                }
            }
        }
        public static void ValidNewRegisterBP()
        {
            string Email = string.Empty;
            int currentRowCount = Convert.ToInt16(DL.Functions.getRowCountBP());

            if (currentRowCount == lastRowCountBP)
            {
                lastRowCountBP = currentRowCount;


                Email = DL.Functions.GetLastRegisterBP();
                //ListaItems = DL.Functions.RecalcGoodsIssue(docnum);
                //DL.Functions.FindItemsBySKUShopify(ListaItems);
               //DL.Functions.SyncOnHandSAPShopifyPlusTransfer(ListaItems);
            }
        }
        public static void ValidNewRegister(string docentry)
        {

        }
        public static void ValidOVShopify(List<DL.listas.OVShopify> lista)
            {
            try
            {
                valoresSAP.DocumentLines = new List<DL.listas.OVSAPDetail>();

                for (global::System.Int32 i = 0; i < lista.Count; i++)
                {
                    string id_customer_sap = DL.Functions.getMetaFieldsByIdCustomerShopify(lista[i].customer_id);
                    valoresSAP.CardName = DL.Functions.getCardNameBysCardCode(id_customer_sap);
                    valoresSAP.CardCode = id_customer_sap;
                    valoresSAP.Comments = "Pedido generado desde el sincronizador";
                    valoresSAP.DocDate = DateTime.Today; 
                    valoresSAP.DocDueDate = DateTime.Today;
                    valoresSAP.U_IdOrdersShopify = lista[i].order_number;
                    valoresSAP.DiscPrcnt = Convert.ToInt16(lista[i].discount.value.Split('.')[0]).ToString();
                    valoresSAP.U_CodPromo = lista[i].discount.code;
                    valoresSAP.U_IL_Timbrar = "N";
                    /* if (string.IsNullOrEmpty(valoresSAP.LicTradNum))
                     {
                         valoresSAP.LicTradNum = "XEXX010101000";
                         valoresSAP.NumAtCard = "XEXX010101000";
                     }*/
                    valoresSAP.NumAtCard = lista[i].order_number;
                    valoresSAP.GroupNum = "2";

                    if (lista[i].payment_gateway_names.Contains("Openpay"))
                    {
                        valoresSAP.PaymentMethod = "03-OP-D";
                    }
                    else if (lista[i].payment_gateway_names.Contains("Pagos en efectivo en OXXO"))
                    {
                        valoresSAP.PaymentMethod = "03-TR-D";
                    }
                    else if (lista[i].payment_gateway_names.Contains("Pago con Tarjeta de Crédito (Con MSI - cuando aplique-) y Tarjeta de Débito"))
                    {
                       //valoresSAP.PeyMethod = "03-TR-D";
                    }
                    else if (lista[i].payment_gateway_names.Contains("manual"))
                    {
                        valoresSAP.PaymentMethod = "03-EF-D";
                    }
                    else if (lista[i].payment_gateway_names.Contains("PayPal"))
                    {
                        valoresSAP.PaymentMethod = "03-PP-D";
                    }

                    //switch (lista[i].payment_gateway_names)
                    //{
                    //    case "Openpay":
                    //        valoresSAP.PeyMethod = "03-OP-D";
                    //        break;
                    //    case "Pagos en efectivo en OXXO":
                    //        valoresSAP.PeyMethod = "03-TR-D";
                    //        break;
                    //    case "Pago con Tarjeta de Crédito (Con MSI - cuando aplique-) y Tarjeta de Débito":
                    //        //valoresSAP.PeyMethod = "03-TR-D";
                    //        break;
                    //    case "manual":
                    //        valoresSAP.PeyMethod = "03-EF-D";
                    //        break;
                    //    case "PayPal":
                    //        valoresSAP.PeyMethod = "03-PP-D";
                    //        break;
                    //    default:
                    //        break;
                    //}
                    //valoresSAP.PaymentMethod = DL.Functions.getPeyMethodCustomerSAP(id_customer_sap);
                    //if (Settings.Default.RFCcondiciones)
                    //{
                    //    valoresSAP.U_RFCondiciones = "04";
                    //}

                    for (global::System.Int32 j = 0; j < lista[i].ditails.Count; j++)
                    {
                        vSAPDetail.ItemCode = DL.Functions.ItemCodeSAPBySKUShopify(lista[i].ditails[j].sku);
                        vSAPDetail.ItemName = DL.Functions.ItemNameSAPBySKUShopify(lista[i].ditails[j].sku);
                        vSAPDetail.BarCode = lista[i].ditails[j].sku;
                        vSAPDetail.Quantity = lista[i].ditails[j].quantity;
                        vSAPDetail.Price = lista[i].ditails[j].price;
                        vSAPDetail.WarehouseCode = Properties.Settings.Default.WhsCode;
                        //vSAPDetail.TaxCode = "";

                        //listaDetalleSAP = new List<DL.listas.OVSAPDetail>();
                        valoresSAP.DocumentLines.Add(vSAPDetail);

                        vSAPDetail = new DL.listas.OVSAPDetail();
                    }
                    Components.DL.Functions.SaveOrderSAP(valoresSAP, lista[i].financial_status);

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
                        string text = reader.ReadToEnd();
                        var model = JsonConvert.DeserializeObject<DL.listas.errores>(text);
                        DL.Functions.Log(model.error.message.value);
                    }
                }
                //Components.DL.Functions.Log(ex.Message);
            }
        }
        public static void CreateInvoices(string DocEntry,DL.listas.OVSAP lista, string estado)
        {
            try
            {
                lista.DocEntry = DocEntry;

                string url = Settings.Default.urlSL + Settings.Default.InvoiceSAP;
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

                    DL.Functions.Log(DateTime.Now + "Factura creada en SAP: " + ovsap.DocNum);
                    lista.DocTotal = ovsap.DocTotal;
                    PaidInvoices(ovsap.DocEntry,lista);
                }
                else
                {
                    var responseString = new
                     StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                    errores err = JsonConvert.DeserializeObject<DL.listas.errores>(responseString);
                    DL.Functions.Log(DateTime.Now + "Error al crear factura: " + err.error.message.value);
                }
            }
            catch (WebException ex)
            {
                //DL.Functions.Log(ex.Message);
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
                        DL.Functions.Log(model.error.message.value);
                    }
                }
            }
        }
        public static void PaidInvoices(string DocEntry, DL.listas.OVSAP lista)
        {

            paymentInvoices.DocDate = DateTime.Today;
            paymentInvoices.CardName = lista.CardName;
            paymentInvoices.CardCode = lista.CardCode;
            paymentInvoices.TaxDate = DateTime.Today;
            //paymentInvoices.DocType = "rCustomer";
            paymentInvoices.PaymentInvoices.LineNum = "0";
            paymentInvoices.PaymentInvoices.DocEntry = DocEntry;
            paymentInvoices.PaymentInvoices.DocNum = lista.DocNum;
            paymentInvoices.PaymentInvoices.SumApplied =lista.DocTotal.ToString();
            //paymentInvoices.PaymentInvoices.SumApplied = lista.doc
            paymentInvoices.PaymentCreditCards.LineNum = "0";
            paymentInvoices.PaymentCreditCards.CreditCard = "3";
            paymentInvoices.PaymentCreditCards.CreditCardNumber = "2834";
            paymentInvoices.PaymentCreditCards.CardValidUntil = "2027-01-31";
            paymentInvoices.PaymentCreditCards.NumOfPayments = "1";
            paymentInvoices.PaymentCreditCards.FirstPaymentDue = "2023-04-12";
            paymentInvoices.PaymentCreditCards.FirstPaymentSum = lista.DocTotal.ToString();
            paymentInvoices.PaymentCreditCards.CreditSum = lista.DocTotal.ToString();
            paymentInvoices.PaymentCreditCards.CreditCur = "MXP";
            paymentInvoices.PaymentCreditCards.CreditRate = "0.0";


            string url = Settings.Default.urlSL + Settings.Default.IncomingPayments;
            var json = JsonConvert.SerializeObject(paymentInvoices);
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

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                var responseString = new
                    StreamReader(httpResponse.GetResponseStream()).ReadToEnd();

                PagoFactura = JsonConvert.DeserializeObject<DL.listas.PagoFactura>(responseString);

                DL.Functions.Log(DateTime.Now + "Factura pagada en SAP: " + PagoFactura.DocNum);
                
            }
            else
            {
                var responseString = new
                 StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                errores err = JsonConvert.DeserializeObject<DL.listas.errores>(responseString);
                DL.Functions.Log(DateTime.Now + "Error al pagar factura: " +docentry + err.error.message.value);
            }

        }
        public static void CreateCustomer(string Nombre, string Apellido, string email, string phone,string cardcode)
        {
            try
            {
                CustomerShopify customerShopify = new CustomerShopify();
                List<CustomerShopify> lista = new List<CustomerShopify>();
                string query = string.Empty;

                customerShopify.customer = new detalle();
                customerShopify.customer.email = email;
                customerShopify.customer.phone = phone;
                customerShopify.customer.first_name = Nombre;
                customerShopify.customer.last_name = Apellido;
                //query = @"{\"customer\": { \"email\":\"" + email + "\"," +
                //        "\"first_name\": \"" + Nombre + "\"," +
                //        "\"last_name\": \"" + Apellido + "\"," +
                //        "\"phone\" : \""+ phone +"\"}}";
                lista.Add(customerShopify);
                customerShopify = new CustomerShopify();
                customerShopify.customer = new detalle();
                var json = JsonConvert.SerializeObject(lista);
                //var content = new StringContent(query.ToString(), Encoding.UTF8, "application/json");
                var content = new StringContent(json.ToString().Replace("[","").Replace("]",""), Encoding.UTF8, "application/json");

                string url = "https://" + Properties.Settings.Default.dominio + "/admin/api/" + Properties.Settings.Default.api_v_shopify +
                    "/customers.json";

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
                            Components.DL.Functions.UpdateSyncBP(cardcode);
                            Components.DL.Functions.Log("Cliente creado: " + Nombre + " " + Apellido);
                        }
                        else
                        {
                            Components.DL.Functions.Log("Error al crear el cliente : " + Nombre + " " + Apellido);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                DL.Functions.Log(ex.Message);
            }
        }

        
    }
}
    
