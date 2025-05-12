using System;
using System.ComponentModel.DataAnnotations;
using Axon_Momo_Service.Features.Common;

namespace Axon_Momo_Service.Features.Collection;

public class CancelInvoiceRequest
{
    public string? ExternalId { get; set; }
}

public class CreateInvoiceRequest
{
    public string? ExternalId { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ValidityDuration { get; set; }
    public Party? IntendedPayer { get; set; }
    public Party? Payee { get; set; }
    public string? Description { get; set; }
}


public class CreatePaymentsRequest
{
    public string? ExternalTransactionId { get; set; }

    public Money? Money { get; set; }

    public string? CustomerReference { get; set; }

    public string? ServiceProviderUserName { get; set; }

    public string? CouponId { get; set; }

    public string? ProductId { get; set; }

    public string? ProductOfferingId { get; set; }

    public string? ReceiverMessage { get; set; }

    public string? SenderNote { get; set; }

    public int? MaxNumberOfRetries { get; set; }

    public bool? IncludeSenderCharges { get; set; }
}



public class Balance
{
    public string? AvailableBalance { get; set; }
    public string? Currency { get; set; }
}

public class PreApprovalDetails
{
    public string PreApprovalId { get; set; } = string.Empty;
    public string ToFri { get; set; } = string.Empty;
    public string FromFri { get; set; } = string.Empty;
    public string FromCurrency { get; set; } = string.Empty;
    public string CreatedTime { get; set; } = string.Empty;
    public string ApprovedTime { get; set; } = string.Empty;
    public string ExpiryTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public string? StartDate { get; set; }
    public string? LastUsedDate { get; set; }
    public string? Offer { get; set; }
    public string? ExternalId { get; set; }
    public string? MaxDebitAmount { get; set; }
}

public class DeliveryNotificationRequest
{
    [Required]
    public string NotificationMessage { get; set; } = string.Empty;
}

public class PreApprovalRequest
{
    public Party Payer { get; set; } = new Party();
    public string PayerCurrency { get; set; } = string.Empty;
    public string PayerMessage { get; set; } = string.Empty;
    public int? ValidityTime { get; set; }
}

public class RequestToPayRequest
{
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; }   = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public Party Payer { get; set; } = new Party();
    public string PayerMessage { get; set; } = string.Empty;
    public string PayeeNote { get; set; } = string.Empty;
}

public class RequestToPayResult
{
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FinancialTransactionId { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public Party Payer { get; set; } = new Party();
    public string PayerMessage { get; set; } = string.Empty;
    public string PayeeNote { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ErrorReason? Reason { get; set; }
}


public class RequestToWithdrawResult
{
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FinancialTransactionId { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public Party Payer { get; set; } = new Party();
    public string PayerMessage { get; set; } = string.Empty;
    public string PayeeNote { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public ErrorReason? Reason { get; set; }
}



public class RequestToWithdrawRequest
{
    public string Amount { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public Party Payer { get; set; } = new Party();
    public string PayerMessage { get; set; } = string.Empty;
    public string PayeeNote { get; set; } = string.Empty;
}






