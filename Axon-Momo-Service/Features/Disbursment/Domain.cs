using System;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Features.Disbursment;

    public class DepositRequest
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
    }

    public class DepositV2Request
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
    }

        public class TransferRequest
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
    }



    public class DisbursmentTransferResult
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string FinancialTransactionId { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
        public string Status { get; set; }
        public ErrorReason? Reason { get; set; }
    }




