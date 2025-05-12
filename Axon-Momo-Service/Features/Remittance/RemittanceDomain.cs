using System;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Features.Remittance;



    public class CashTransferRequest
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string ExternalId { get; set; } = string.Empty;
        public string OrginatingCountry { get; set; } = string.Empty;
        public string OriginalAmount { get; set; } = string.Empty;
        public string OriginalCurrency { get; set; } = string.Empty;
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
        public string PayerIdentificationType { get; set; } = string.Empty;
        public string PayerIdentificationNumber { get; set; } = string.Empty;
        public string PayerIdentity { get; set; } = string.Empty;
        public string PayerFirstName { get; set; } = string.Empty;
        public string PayerSurName { get; set; } = string.Empty;
        public string PayerLanguageCode { get; set; } = string.Empty;
        public string PayerEmail { get; set; } = string.Empty;
        public string PayerMsisdn { get; set; } = string.Empty;
        public string PayerGender { get; set; } = string.Empty;
    }

        public class CashTransferResult
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

        public class TransferResult
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

    public class RemittanceTransferRequest
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public Party Payee { get; set; } = new Party();
        public string PayerMessage { get; set; } = string.Empty;
        public string PayeeNote { get; set; } = string.Empty;
    }

