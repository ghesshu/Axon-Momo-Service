# CollectionController API Documentation

This document outlines the endpoints in the `CollectionController` for a mock collection service API. Each endpoint is described with its purpose, required inputs, expected outputs, and specific scenarios that trigger particular responses (including error codes). This is designed to help developers understand how to interact with the API and simulate responses for testing or presentation purposes.

---

## 1. BC Authorize
**Endpoint**: `POST /api/Collection/v1_0/bc-authorize`  
**Purpose**: Initiates an authorization request for a business client (BC) to access collection services.

### Required Inputs
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (e.g., sandbox).
  - `X-Callback-Url` (optional): Callback URL for asynchronous notifications.
- **Body** (Form Data):
  - `Scope`: The scope of access (required, non-empty string).
  - `LoginHint`: User identifier hint (required, non-empty string).
  - `AccessType` (optional): Must be either "online" or "offline" if provided.
- **Services** (Injected):
  - `AuthContext`: Context for authentication checks.

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  {
    "auth_req_id": "<GUID>",
    "interval": 5,
    "expires_in": 600
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Invalid or missing `Scope` or `LoginHint`):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing scope or login_hint in request body."
    }
    ```
  - **400 Bad Request** (Invalid `AccessType`):
    ```json
    {
      "Code": "INVALID_ACCESS_TYPE",
      "Message": "access_type must be 'online' or 'offline'."
    }
    ```

### Specific Scenarios
- Send a request with missing `Scope` or `LoginHint` to trigger `INVALID_REQUEST`.
- Set `AccessType` to anything other than "online" or "offline" (e.g., "invalid") to trigger `INVALID_ACCESS_TYPE`.

---

## 2. Create Access Token
**Endpoint**: `POST /api/Collection/token`  
**Purpose**: Generates an access token based on an authorization request ID.

### Required Inputs
- **Headers**:
  - `Authorization`: Contains the `auth_req_id` (required, must be a valid GUID).
- **Services** (Injected):
  - `AuthContext`: Context for authentication checks.
  - `DataContext`: Database context to query `AuthTokens`.

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  {
    "access_token": "<GUID>",
    "token_type": "Bearer",
    "expires_in": 3600
  }
  ```
- **Error Responses**:
  - **401 Unauthorized** (Missing or empty `Authorization` header):
    ```json
    {
      "Code": "UNAUTHORIZED",
      "Message": "Invalid or missing authorization header."
    }
    ```
  - **401 Unauthorized** (Invalid `auth_req_id` format):
    ```json
    {
      "Code": "INVALID_AUTH_REQ_ID",
      "Message": "Invalid auth_req_id format."
    }
    ```
  - **401 Unauthorized** (Token not found in database):
    ```json
    {
      "Code": "INVALID_TOKEN",
      "Message": "Token not found."
    }
    ```
  - **401 Unauthorized** (Token expired):
    ```json
    {
      "Code": "TOKEN_EXPIRED",
      "Message": "Session has expired."
    }
    ```

### Specific Scenarios
- Send an empty `Authorization` header to trigger `UNAUTHORIZED`.
- Send a non-GUID `Authorization` value (e.g., "invalid") to trigger `INVALID_AUTH_REQ_ID`.
- Use a GUID that doesn’t exist in the `AuthTokens` table to trigger `INVALID_TOKEN`.
- Use an expired token (where `Expires_in` < current Unix timestamp) to trigger `TOKEN_EXPIRED`.

---

## 3. Create OAuth2 Token
**Endpoint**: `POST /api/Collection/oauth2/token`  
**Purpose**: Generates an OAuth2 token for authenticated access to collection services.

### Required Inputs
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (Form Data):
  - `GrantType`: The grant type for the token request (required, non-empty string).
- **Services** (Injected):
  - `AuthContext`: Context for authentication checks.

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  {
    "access_token": "<GUID>",
    "token_type": "Bearer",
    "expires_in": 3600,
    "scope": "collection",
    "refresh_token": "<GUID>",
    "refresh_token_expired_in": 86400
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or invalid `GrantType`):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing grant_type in request body."
    }
    ```

### Specific Scenarios
- Omit `GrantType` or send an empty string to trigger `INVALID_REQUEST`.

---

## 4. Create Payments
**Endpoint**: `POST /api/Collection/v2_0/payment`  
**Purpose**: Initiates a payment request.

### Required Inputs
- **Headers**:
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the payment (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Money`: Object containing:
    - `Amount`: Payment amount (required, non-empty string).
    - `Currency`: Currency code (required, non-empty string).
  - `CustomerReference` (optional): Reference for the customer.

### Expected Outputs
- **Success Response** (202 Accepted):
  ```json
  {}
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or invalid `Money.Amount` or `Money.Currency`):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing required fields in request body (money.amount, money.currency)."
    }
    ```
  - **400 Bad Request** (Invalid `CustomerReference`):
    ```json
    {
      "Code": "INVALID_CUSTOMER_REFERENCE",
      "Message": "Invalid customer reference provided."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (`X-Reference-Id` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `Money.Amount` or `Money.Currency` to trigger `INVALID_REQUEST`.
- Set `CustomerReference` to "invalid" to trigger `INVALID_CUSTOMER_REFERENCE`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `X-Reference-Id` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.

---

## 5. Get Account Balance
**Endpoint**: `GET /api/Collection/v1_0/account/balance`  
**Purpose**: Retrieves the account balance.

### Required Inputs
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  {
    "AvailableBalance": "1000.00",
    "Currency": "GHS"
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or empty `X-Target-Environment`):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "X-Target-Environment is required and must not be empty."
    }
    ```
  - **500 Internal Server Error** (`X-Target-Environment` = "invalid"):
    ```json
    {
      "Code": "NOT_ALLOWED_TARGET_ENVIRONMENT",
      "Message": "Access to target environment is forbidden."
    }
    ```
  - **500 Internal Server Error** (`X-Target-Environment` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `X-Target-Environment` or send an empty string to trigger `INVALID_REQUEST`.
- Set `X-Target-Environment` to "invalid" to trigger `NOT_ALLOWED_TARGET_ENVIRONMENT`.
- Set `X-Target-Environment` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.

---

## 6. Request To Pay
**Endpoint**: `POST /api/Collection/v1_0/requesttopay`  
**Purpose**: Initiates a request to pay transaction.

### Required Inputs
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the transaction (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Transaction amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payer`: Object containing:
    - `PartyId`: Payer identifier (required, non-empty string).
    - `PartyIdType`: Type of payer ID (required, non-empty string).

### Expected Outputs
- **Success Response** (202 Accepted):
  ```json
  {}
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or invalid required fields):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing required fields in request body (amount, currency, payer)."
    }
    ```
  - **400 Bad Request** (Invalid payer):
    ```json
    {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payer not found."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (`X-Reference-Id` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payer.PartyId`, or `Payer.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `Payer.PartyId` to "invalid" to trigger `PAYEE_NOT_FOUND`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `X-Reference-Id` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.

---

## 7. Request To Withdraw (V1)
**Endpoint**: `POST /api/Collection/v1_0/requesttowithdraw`  
**Purpose**: Initiates a request to withdraw transaction (version 1).

### Required Inputs
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the transaction (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Transaction amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payer`: Object containing:
    - `PartyId`: Payer identifier (required, non-empty string).
    - `PartyIdType`: Type of payer ID (required, non-empty string).

### Expected Outputs
- **Success Response** (202 Accepted):
  ```json
  {}
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or invalid required fields):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing required fields in request body (amount, currency, payer)."
    }
    ```
  - **400 Bad Request** (Invalid payer):
    ```json
    {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payer not found."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (`X-Reference-Id` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payer.PartyId`, or `Payer.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `Payer.PartyId` to "invalid" to trigger `PAYEE_NOT_FOUND`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `X-Reference-Id` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.

---

## 8. Request To Withdraw (V2)
**Endpoint**: `POST /api/Collection/v2_0/requesttowithdraw`  
**Purpose**: Initiates a request to withdraw transaction (version 2).

### Required Inputs
- **Headers**:
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the transaction (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Transaction amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payer`: Object containing:
    - `PartyId`: Payer identifier (required, non-empty string).
    - `PartyIdType`: Type of payer ID (required, non-empty string).

### Expected Outputs
- **Success Response** (202 Accepted):
  ```json
  {}
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or invalid required fields):
    ```json
    {
      "Code": "INVALID_REQUEST",
      "Message": "Invalid or missing required fields in request body (amount, currency, payer)."
    }
    ```
  - **400 Bad Request** (Invalid payer):
    ```json
    {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payer not found."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (`X-Reference-Id` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payer.PartyId`, or `Payer.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `Payer.PartyId` to "invalid" to trigger `PAYEE_NOT_FOUND`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `X-Reference-Id` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.

---

## 9. Request To Pay Transaction Status
**Endpoint**: `GET /api/Collection/collection/v1_0/requesttopay/{referenceId}`  
**Purpose**: Retrieves the status of a request to pay transaction by reference ID.

### Required Inputs
- **Route Parameter `_referenceId_`:
  - `referenceId`: Unique reference ID of the transaction (required).
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK, `referenceId` = "success"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "FinancialTransactionId": "23503452",
    "Status": "SUCCESSFUL"
  }
  ```
- **Failed Response** (200 OK, `referenceId` = "payernotfound"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "Status": "FAILED",
    "Reason": {
      "Code": "PAYER_NOT_FOUND",
      "Message": "Payee does not exist"
    }
  }
  ```
- **Pending Response** (200 OK, default case):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "Status": "PENDING",
    "PayerMessage": "Pending authorization",
    "PayeeNote": "Awaiting payer approval",
    "Reason": {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payee not found, transaction pending."
    }
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or empty `referenceId`):
    ```json
    {
      "Code": "INVALID_REFERENCE_ID",
      "Message": "referenceId is required and must not be empty."
    }
    ```
  - **404 Not Found** (`referenceId` = "notfound"):
    ```json
    {
      "Code": "RESOURCE_NOT_FOUND",
      "Message": "Requested resource was not found."
    }
    ```
  - **500 Internal Server Error** (`referenceId` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `referenceId` or send an empty string to trigger `INVALID_REFERENCE_ID`.
- Set `referenceId` to "notfound" to trigger `RESOURCE_NOT_FOUND`.
- Set `referenceId` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `referenceId` to "success" to trigger a successful transaction response.
- Set `referenceId` to "payernotfound" to trigger `PAYER_NOT_FOUND`.
- Use any other `referenceId` to trigger the default `PENDING` response with `PAYEE_NOT_FOUND`.

---

## 10. Request To Withdraw Transaction Status
**Endpoint**: `GET /api/Collection/v1_0/requesttowithdraw/{referenceId}`  
**Purpose**: Retrieves the status of a request to withdraw transaction by reference ID.

### Required Inputs
- **Route Parameter**:
  - `referenceId`: Unique reference ID of the transaction (required).
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK, `referenceId` = "success"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "FinancialTransactionId": "23503452",
    "Status": "SUCCESSFUL"
  }
  ```
- **Failed Response** (200 OK, `referenceId` = "payernotfound"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "Status": "FAILED",
    "Reason": {
      "Code": "PAYER_NOT_FOUND",
      "Message": "Payee does not exist"
    }
  }
  ```
- **Pending Response** (200 OK, default case):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "947354",
    "Payer": {
      "PartyIdType": "MSISDN",
      "PartyId": "4656473839"
    },
    "Status": "PENDING",
    "PayerMessage": "Pending authorization",
    "PayeeNote": "Awaiting payer approval",
    "Reason": {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payee not found, transaction pending."
    }
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing or empty `referenceId`):
    ```json
    {
      "Code": "INVALID_REFERENCE_ID",
      "Message": "referenceId is required and must not be empty."
    }
    ```
  - **404 Not Found** (`referenceId` = "notfound"):
    ```json
    {
      "Code": "RESOURCE_NOT_FOUND",
      "Message": "Requested resource was not found."
    }
    ```
  - **500 Internal Server Error** (`referenceId` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `referenceId` or send an empty string to trigger `INVALID_REFERENCE_ID`.
- Set `referenceId` to "notfound" to trigger `RESOURCE_NOT_FOUND`.
- Set `referenceId` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `referenceId` to "success" to trigger a successful transaction response.
- Set `referenceId` to "payernotfound" to trigger `PAYER_NOT_FOUND`.
- Use any other `referenceId` to trigger the default `PENDING` response with `PAYEE_NOT_FOUND`.

---

## 11. Validate Account Holder Status
**Endpoint**: `GET /api/Collection/v1_0/accountholder/{accountHolderIdType}/{accountHolderId}/active`  
**Purpose**: Checks if an account holder is active based on their ID type and ID.

### Required Inputs
- **Route Parameters**:
  - `accountHolderIdType`: Type of account holder ID (required, must be "msisdn" or "email", lowercase).
  - `accountHolderId`: Account holder identifier (required).
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  true
  ```
- **Inactive/Not Found Response** (200 OK, `accountHolderId` = "inactive" or "notfound"):
  ```json
  false
  ```
- **Error Responses**:
  - **400 Bad Request** (Invalid or missing `accountHolderIdType`):
    ```json
    {
      "Code": "INVALID_ACCOUNT_HOLDER_ID_TYPE",
      "Message": "accountHolderIdType must be 'msisdn' or 'email' (lowercase)."
    }
    ```
  - **400 Bad Request** (Missing `accountHolderId`):
    ```json
    {
      "Code": "INVALID_ACCOUNT_HOLDER_ID",
      "Message": "accountHolderId is required and must not be empty."
    }
    ```
  - **400 Bad Request** (Invalid MSISDN format):
    ```json
    {
      "Code": "INVALID_MSISDN",
      "Message": "accountHolderId must be a valid MSISDN (e.g., +1234567890)."
    }
    ```
  - **400 Bad Request** (Invalid email format):
    ```json
    {
      "Code": "INVALID_EMAIL",
      "Message": "accountHolderId must be a valid email address."
    }
    ```
  - **500 Internal Server Error** (`X-Target-Environment` = "invalid"):
    ```json
    {
      "Code": "NOT_ALLOWED_TARGET_ENVIRONMENT",
      "Message": "Access to target environment is forbidden."
    }
    ```
  - **500 Internal Server Error** (`accountHolderId` = "error"):
    ```json
    {
      "Code": "INTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `accountHolderIdType` or set to an invalid value (e.g., "invalid", "MSISDN") to trigger `INVALID_ACCOUNT_HOLDER_ID_TYPE`.
- Omit `accountHolderId` to trigger `INVALID_ACCOUNT_HOLDER_ID`.
- Set `accountHolderIdType` to "msisdn" and `accountHolderId` to an invalid format (e.g., "123") to trigger `INVALID_MSISDN`.
- Set `accountHolderIdType` to "email" and `accountHolderId` to an invalid format (e.g., "invalid") to trigger `INVALID_EMAIL`.
- Set `X-Target-Environment` to "invalid" to trigger `NOT_ALLOWED_TARGET_ENVIRONMENT`.
- Set `accountHolderId` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `accountHolderId` to "inactive" or "notfound" to return `false`.
- Use a valid `accountHolderId` (e.g., "+1234567890" for msisdn, "test@example.com" for email) to return `true`.

---

This documentation provides a clear guide for developers to test the `CollectionController` endpoints, including how to trigger specific success and error responses for presentation or mock-up purposes.