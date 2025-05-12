using System;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Features.Disbursment;

    public class DepositRequest
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
    }

    public class DepositV2Request
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
    }

        public class TransferRequest
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; }  = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
    }



    public class DisbursmentTransferResult
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string FinancialTransactionId { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public ErrorReason? Reason { get; set; }
    }




