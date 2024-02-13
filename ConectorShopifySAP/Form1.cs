using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConectorShopifySAP
{
    public partial class frmInicio : Form
    {
        #region Variables
        private static int lastRowCount, lastRowCountOINV = 0;
        private static string fechaOV;
        static Thread synShopifySAP;
        //Thread synShopifySAP
        //string fecha = dtp_Fecha.Text.ToString();
        #endregion
        public frmInicio()
        {
            InitializeComponent();
        }

        private void frmInicio_Load(object sender, EventArgs e)
        {
            btnSync.Visible = false;
            button1.Visible = false;
            button2.Visible = false;

            toolStripStatusLabel1.Text = "Sincronizador Shopify - SAP Dermaexpress";

            ////Sincronizador de SAP a Shopify
            //System.Timers.Timer timer_Invoice = new System.Timers.Timer(50000000);
            //timer_Invoice.Elapsed += CheckForChangesOINV;
            //timer_Invoice.AutoReset = true;
            //timer_Invoice.Start();

            //Thread synShopifySAP = new Thread(new ThreadStart(SincShopifySAPOV));
            //synShopifySAP.Start();
            //synShopifySAP.Join();

        }

        private void button2_Click(object sender, EventArgs e)
        {
           

        }
        private static void CheckForChangesOINV(object sender, EventArgs e)
        {
           Components.BL.Functions.ValidNewRegisterOINV();
        }
        private static void SincShopifySAPOV()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(1000 * 60 * 1);
            timer_Invoice.Elapsed += SynShopifySAPOV;
            timer_Invoice.AutoReset = false;
            timer_Invoice.Start();
        }
        private static void SyncOINV()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(15000);
            timer_Invoice.Elapsed += CheckForChangesOINV;
            timer_Invoice.AutoReset = true;
            timer_Invoice.Start();
        }
        private static void SynShopifySAPOV(object sender, EventArgs e)
        {
            //Components.DL.Functions.ConexionSL();
            Components.DL.Functions.ConexionODO();

            //Components.BL.Functions.initShopify();
            //Components.BL.Functions.getOrderShopifyByDate(DateTime.Now.ToString("yyyy-MM-dd"));
            //Components.BL.Functions.ValidNewRegisterOINV();
            Components.BL.Functions.initShopify();
        }
        private static void SynShopifySAPOVForDate(object sender, EventArgs e)
        {
            Components.DL.Functions.ConexionSL();
            Components.DL.Functions.ConexionODO();
            //Components.BL.Functions.initShopify();
            //Components.BL.Functions.getOrderShopifyByDate(dateTimePicker1.Text.ToString("yyyy-MM-dd"));
            //Components.BL.Functions.ValidNewRegisterOINV();
            Components.BL.Functions.getOrderShopifyByDate(fechaOV);

        }
        private static void SynShopifySAPOVForDateThread()
        {
            //Components.DL.Functions.ConexionSL();
            //Components.DL.Functions.ConexionODO();
            ////Components.BL.Functions.initShopify();
            //Components.BL.Functions.getOrderShopifyByDate(DateTime.Now.ToString("yyyy-MM-dd"));
            //Components.BL.Functions.ValidNewRegisterOINV();

            System.Timers.Timer timer_syncSAPOV = new System.Timers.Timer(1000 * 60 * 1);
            timer_syncSAPOV.Elapsed += SynShopifySAPOVForDate;
            timer_syncSAPOV.AutoReset = false;
            timer_syncSAPOV.Start();
        }
        private static void SyncShopifyGoodsEntrys(object sender, EventArgs e)
        {
            try
            {
                Components.DL.Functions.ConexionSL();
                Components.DL.Functions.ConexionODO();
                Components.BL.Functions.ValidNewRegisterGoodsEntrys();
                //System.Timers.Timer timer_Invoice = new System.Timers.Timer(50000000);
                //timer_Invoice.Elapsed += CheckForChangesOINV;
                //timer_Invoice.AutoReset = true;
                //timer_Invoice.Start();
            }
            catch (Exception ex)
            {
                Components.DL.Functions.Log(ex.Message);
            }
        }
        private static void SyncShopifyGoodsEntrysThread()
        {
            try
            {
                //Components.DL.Functions.ConexionSL();
                //Components.DL.Functions.ConexionODO();
                //Components.BL.Functions.ValidNewRegisterGoodsEntrys();
                System.Timers.Timer timer_Invoice = new System.Timers.Timer(15000);
                timer_Invoice.Elapsed += SyncShopifyGoodsEntrys;
                timer_Invoice.AutoReset = true;
                timer_Invoice.Start();
            }
            catch (Exception ex)
            {
                Components.DL.Functions.Log(ex.Message);
            }
        }
        public static void SyncShopifyPricesThread()
        {
            try
            {
                System.Timers.Timer timer_Invoice = new System.Timers.Timer(22000);
                timer_Invoice.Elapsed += SyncShopifyPrices;
                timer_Invoice.AutoReset = true;
                timer_Invoice.Start();
            }
            catch (Exception ex)
            {
                Components.DL.Functions.Log(ex.Message);
            }
        }
        public static void SyncShopifyPrices(object sender, EventArgs e)
        {
            try
            {
                Components.DL.Functions.ConexionSL();
                Components.DL.Functions.ConexionODO();
                Components.BL.Functions.SyncPrices();

            }
            catch (Exception ex)
            {
                Components.DL.Functions.Log(ex.Message);
            }
        }

        private static void SyncInventoryTransfer(object sender, EventArgs e)
        {
            Components.DL.Functions.ConexionSL();
            Components.DL.Functions.ConexionODO();
            Components.BL.Functions.ValidNewRegisterInventoryTransfer();

        }
        private static void SyncInventoryTransferThread()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(12000);
            timer_Invoice.Elapsed += SyncInventoryTransfer;
            timer_Invoice.AutoReset = true;
            timer_Invoice.Start();

        }
        void SyncSapShopifyAllItems(object sender, EventArgs e)
        {
            if (!synShopifySAP.IsAlive)
            {
                Components.BL.Functions.SyncAllItemsSapToShopify();
                this.Invoke(new Action(() => { MessageBox.Show(this, "Se termino de sincronizar todos los artículos","Sincornización de artículos," 
                    ,MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1); })); 
            }
        }
        private void SyncSapShopifyAllItemsThread()
        {
                System.Timers.Timer timer_Invoice = new System.Timers.Timer(1000 * 60 * 1);
            //if (synShopifySAP.IsAlive)
            //{

                timer_Invoice.Elapsed += SyncSapShopifyAllItems;
                timer_Invoice.AutoReset = false;
                timer_Invoice.Start();
            //}
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            //System.Timers.Timer timer_Invoice = new System.Timers.Timer(10000);
            //timer_Invoice.Elapsed += SyncSapShopifyAllItems;
            //timer_Invoice.AutoReset = true;
            //timer_Invoice.Start();
        }

        private void rjButton1_Click(object sender, EventArgs e)
        {
          
            Thread synShopifySAP = new Thread(new ThreadStart(SyncSapShopifyAllItemsThread));
            synShopifySAP.Start();
            synShopifySAP.Join();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void rjButton2_Click(object sender, EventArgs e)
        {
            //Thread synShopifySAP = new Thread(new ThreadStart(SyncSapShopifyAllItemsThread));
            //synShopifySAP.Start();
            //synShopifySAP.Join();
            fechaOV = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            Thread synShopifySAP = new Thread(new ThreadStart(SynShopifySAPOVForDateThread));
            synShopifySAP.Start();
            synShopifySAP.Join();
            synShopifySAP.Abort();
        }

        private void rjButton3_Click(object sender, EventArgs e)
        {
            //Components.BL.Functions.ValidNewRegisterOINV();
            ///sincronizar los pedidos de shopify a SAP
            //Components.BL.Functions.getOrderShopifyByDate(DateTime.Now.ToString("yyyy-MM-dd"));
            //Components.DL.Functions.ConexionSL();
            Components.DL.Functions.ConexionODO();

            //Components.BL.Functions.initShopify();


            //Sincronizador de SAP a Shopify
            //Thread syncShopifySAPOINV = new Thread(new ThreadStart(SincShopifySAPOV));
            //syncShopifySAPOINV.Start();
            //syncShopifySAPOINV.Join();

            //Ordenes de venta
            Thread synShopifySAP = new Thread(new ThreadStart(SincShopifySAPOV));
            synShopifySAP.Start();
            synShopifySAP.Join();

            //Entradas de mercancia
            Thread synEntradasShopifySAP = new Thread(new ThreadStart(SyncShopifyGoodsEntrysThread));
            synEntradasShopifySAP.Start();
            synEntradasShopifySAP.Join();

            //Transferencia
            Thread synTransferenciaShopifySAP = new Thread(new ThreadStart(SyncInventoryTransferThread));
            synTransferenciaShopifySAP.Start();
            synTransferenciaShopifySAP.Join();

            //Salida de mercancia
            Thread synGoodsIssueShopifySAP = new Thread(new ThreadStart(SyncGoodsIssueThread));
            synGoodsIssueShopifySAP.Start();
            synGoodsIssueShopifySAP.Join();

            //Orden de venta por fecha
            //Thread synGoodsIssueShopifySAP = new Thread(new ThreadStart(SyncInventoryTransferThread));
            //synShopifySAP.Start();
            //synShopifySAP.Join();
            
            //Sincronizacion de BP
            Thread syncBP = new Thread(new ThreadStart(SyncBPThread));
            syncBP.Start();
            syncBP.Join();

            //Sincronizacion de precios
            Thread syncPrices = new Thread(new ThreadStart(SyncShopifyPricesThread));
            syncPrices.Start();
            syncPrices.Join();
        }

        private static void SyncInventoryTransferless(object sender,EventArgs e)
        {
            Components.BL.Functions.ValirNewRegisterGoodsIssue();
        }
        private static void SyncOV(object sender, EventArgs e)
        {
            Components.BL.Functions.initShopify();
        }
        private static void SyncGoodsIssue(object sender, EventArgs e)
        {
            Components.BL.Functions.ValirNewRegisterGoodsIssue();
        }
        private static void SyncBP(object sender, EventArgs e)
        {
            Components.BL.Functions.ValidNewRegisterBP();
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void rjButton6_Click(object sender, EventArgs e)
        {
            //Components.BL.Functions.SyncAllItemsSapToShopify();

            synShopifySAP = new Thread(new ThreadStart(SyncSapShopifyAllItemsThread));

            synShopifySAP.Start();
            synShopifySAP.Join();
            synShopifySAP.Abort();

        }
        private void rjButton7_Click(object sender, EventArgs e)
        {
            //Components.DL.Functions.getListCustomerShopify();
            //Components.DL.Functions.GetCustomerSAP();

            synShopifySAP = new Thread(new ThreadStart(GetCustomer));
            if (!synShopifySAP.IsAlive)
            {
                synShopifySAP.Start();
                synShopifySAP.Join();
            }
        }
        private static void SyncOVThread()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(13000);
            timer_Invoice.Elapsed += SyncGoodsIssue;
            timer_Invoice.AutoReset = true;
            timer_Invoice.Start();
        }
        private static void GetCustomer()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(20000);
            if (synShopifySAP.IsAlive)
            {
                timer_Invoice.Elapsed += SyncCustonerThread;
                timer_Invoice.AutoReset = false;
                timer_Invoice.Start();
                //synShopifySAP.Join();
            }
           
        }
        private static void SyncCustonerThread(object sender,EventArgs e) {

            Components.DL.Functions.GetCustomerSAP();
        }
        private static void SyncGoodsIssueThread()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(13000);
            timer_Invoice.Elapsed += SyncGoodsIssue;
            timer_Invoice.AutoReset = true;
            timer_Invoice.Start();
            //Components.BL.Functions.ValirNewRegisterGoodsIssue();
        }
        private static void SyncBPThread()
        {
            System.Timers.Timer timer_Invoice = new System.Timers.Timer(20000);
            timer_Invoice.Elapsed += SyncBP;
            timer_Invoice.AutoReset = true;
            timer_Invoice.Start();
        }
    }
}
