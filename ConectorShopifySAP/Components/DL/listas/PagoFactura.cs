using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ConectorShopifySAP.Components.DL.listas
{
    internal class PagoFactura
    {
        //POST https://localhost:50000/b1s/v1/IncomingPayments
        public string DocNum { get; set; }
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string DocType { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime TaxDate { get; set; }
        public PaymentInvoices PaymentInvoices { get; set; }
        public PaymentCreditCards  PaymentCreditCards { get; set; }
        public ElectronicProtocols ElectronicProtocols { get; set; }


    }
    internal class PaymentInvoices
    {
        public string LineNum { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string SumApplied { get; set; }
        public string PaymentMethodCode { get; set; }   
        public string NumOfPayments { get; set; }
        public string FirstPaymentDue { get; set; }
        public string FirstPaymentSum { get; set; }
        public string CreditSum { get; set; }


    }
    internal class PaymentCreditCards
    {
       
        public string LineNum { get; set; } = "0";
        public string CreditCard { get; set; }
        public string CreditCardNumber { get; set; }
        public string CardValidUntil { get; set; }
        public string PaymentMethodCode { get; set; }
        public string CreditSum { get; set; }
        public string CreditCur { get; set; } = "MXP";
        public string CreditType { get; set; }
        public string NumOfPayments { get; set; }
        public string FirstPaymentDue { get; set; }
        public string FirstPaymentSum { get; set; }
        public string CreditRate { get; set; }
    }
    internal class ElectronicProtocols
    {
        public string ProtocolCode { get; set; }
        public string GenerationType { get; set; }
        public string TestingMode { get; set; }
    }
}
