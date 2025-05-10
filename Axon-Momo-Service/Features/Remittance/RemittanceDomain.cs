using System;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Features.Remittance;



    public class CashTransferRequest
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public Party Payee { get; set; }
        public string ExternalId { get; set; }
        public string OrginatingCountry { get; set; }
        public string OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
        public string PayerIdentificationType { get; set; }
        public string PayerIdentificationNumber { get; set; }
        public string PayerIdentity { get; set; }
        public string PayerFirstName { get; set; }
        public string PayerSurName { get; set; }
        public string PayerLanguageCode { get; set; }
        public string PayerEmail { get; set; }
        public string PayerMsisdn { get; set; }
        public string PayerGender { get; set; }
    }

        public class CashTransferResult
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string FinancialTransactionId { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
        public string Status { get; set; }
        public ErrorReason Reason { get; set; }
    }

        public class TransferResult
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string FinancialTransactionId { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
        public string Status { get; set; }
        public ErrorReason Reason { get; set; }
    }

    public class RemittanceTransferRequest
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExternalId { get; set; }
        public Party Payee { get; set; }
        public string PayerMessage { get; set; }
        public string PayeeNote { get; set; }
    }

