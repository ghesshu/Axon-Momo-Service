# DisbursmentController API Documentation

This document outlines the endpoints in the `DisbursmentController` for a mock disbursement service API. Each endpoint is described with its purpose, required inputs, expected outputs, and specific scenarios that trigger particular responses (including error codes). This is designed to help developers understand how to interact with the API and simulate responses for testing or presentation purposes.

---

## 1. BC Authorize
**Endpoint**: `POST /api/Disbursment/v1_0/bc-authorize`  
**Purpose**: Initiates an authorization request for a business client (BC) to access disbursement services.

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
  - **401 Unauthorized** (Authentication failure):
    ```json
    {
      "Code": "UNAUTHORIZED",
      "Message": "Invalid or missing authorization header."
    }
    ```

### Specific Scenarios
- Send a request with missing `Scope` or `LoginHint` to trigger `INVALID_REQUEST`.
- Set `AccessType` to anything other than "online" or "offline" (e.g., "invalid") to trigger `INVALID_ACCESS_TYPE`.
- Fail authentication (e.g., missing or invalid credentials in `AuthContext`) to trigger `UNAUTHORIZED`.

---

## 2. Create OAuth2 Token
**Endpoint**: `POST /api/Disbursment/oauth2/token`  
**Purpose**: Generates an OAuth2 token for authenticated access to disbursement services.

### Required Inputs
- **Headers**:
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
  - **401 Unauthorized** (Authentication failure):
    ```json
    {
      "Code": "UNAUTHORIZED",
      "Message": "Invalid or missing authorization header."
    }
    ```

### Specific Scenarios
- Omit `GrantType` or send an empty string to trigger `INVALID_REQUEST`.
- Fail authentication (e.g., missing or invalid credentials in `AuthContext`) to trigger `UNAUTHORIZED`.

---

## 3. Create Access Token
**Endpoint**: `POST /api/Disbursment/token`  
**Purpose**: Generates an access token based on an authorization request ID.

### Required Inputs
- **Headers**:
  - `Authorization`: Contains the `auth_req_id` (required, must be a valid GUID).
- **Services** (Injected):
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

## 4. Create Deposit (V1)
**Endpoint**: `POST /api/Disbursment/collection/v1_0/deposit`  
**Purpose**: Initiates a deposit to a payee (version 1).

### Required Inputs
- **Headers**:
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the deposit (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Deposit amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payee`: Object containing:
    - `PartyId`: Payee identifier (required, non-empty string).
    - `PartyIdType`: Type of payee ID (required, non-empty string).

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
      "Message": "Invalid or missing required fields in request body."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (Invalid currency):
    ```json
    {
      "Code": "INVALID_CURRENCY",
      "Message": "Currency not supported."
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payee.PartyId`, or `Payee.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `Currency` to anything other than "GHS" (e.g., "USD") to trigger `INVALID_CURRENCY`.

---

## 5. Create Deposit (V2)
**Endpoint**: `POST /api/Disbursment/collection/v2_0/deposit`  
**Purpose**: Initiates a deposit to a payee (version 2).

### Required Inputs
- **Headers**:
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the deposit (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Deposit amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payee`: Object containing:
    - `PartyId`: Payee identifier (required, non-empty string).
    - `PartyIdType`: Type of payee ID (required, non-empty string).

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
      "Message": "Invalid or missing required fields in request body."
    }
    ```
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "Code": "RESOURCE_ALREADY_EXIST",
      "Message": "Duplicated reference id. Creation of resource failed."
    }
    ```
  - **500 Internal Server Error** (Invalid currency):
    ```json
    {
      "Code": "INVALID_CURRENCY",
      "Message": "Currency not supported."
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payee.PartyId`, or `Payee.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `Currency` to anything other than "GHS" (e.g., "USD") to trigger `INVALID_CURRENCY`.

---

## 6. Create Transfer
**Endpoint**: `POST /api/Disbursment/v1_0/transfer`  
**Purpose**: Initiates a disbursement transfer to a payee.

### Required Inputs
- **Headers**:
  - `X-Callback-Url` (optional): Callback URL for notifications.
  - `X-Reference-Id`: Unique reference ID for the transfer (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (JSON):
  - `Amount`: Transfer amount (required, non-empty string).
  - `Currency`: Currency code (required, non-empty string).
  - `Payee`: Object containing:
    - `PartyId`: Payee identifier (required, non-empty string).
    - `PartyIdType`: Type of payee ID (required, non-empty string).

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
      "Message": "Invalid or missing required fields in request body."
    }
    ```
  - **400 Bad Request** (Invalid currency):
    ```json
    {
      "Code": "INVALID_CURRENCY",
      "Message": "Currency not supported."
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
- Omit `Amount`, `Currency`, `Payee.PartyId`, or `Payee.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `X-Reference-Id` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `Currency` to anything other than "GHS" (e.g., "USD") to trigger `INVALID_CURRENCY`.

---

## 7. Get Transfer Status
**Endpoint**: `GET /api/Disbursment/v1_0/transfer/{referenceId}`  
**Purpose**: Retrieves the status of a disbursement transfer by reference ID.

### Required Inputs
- **Route Parameter**:
  - `referenceId`: Unique reference ID of the transfer (required).
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK, `referenceId` = "success"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "FinancialTransactionId": "363440463",
    "ExternalId": "83453",
    "Payee": {
      "PartyIdType": "MSISDN",
      "PartyId": "4609274685"
    },
    "Status": "SUCCESSFUL"
  }
  ```
- **Failed Response** (200 OK, `referenceId` = "limitbreached"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "83453",
    "Payee": {
      "PartyIdType": "MSISDN",
      "PartyId": "4609274685"
    },
    "Status": "FAILED",
    "Reason": {
      "Code": "PAYER_LIMIT_REACHED",
      "Message": "The payer's limit has been breached."
    }
  }
  ```
- **Failed Response** (200 OK, `referenceId` = "insufficient"):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "83453",
    "Payee": {
      "PartyIdType": "MSISDN",
      "PartyId": "4609274685"
    },
    "Status": "FAILED",
    "Reason": {
      "Code": "NOT_ENOUGH_FUNDS",
      "Message": "The payer does not have enough funds."
    }
  }
  ```
- **Pending Response** (200 OK, default case):
  ```json
  {
    "Amount": "100",
    "Currency": "GHS",
    "ExternalId": "83453",
    "Payee": {
      "PartyIdType": "MSISDN",
      "PartyId": "4609274685"
    },
    "Status": "PENDING",
    "PayerMessage": "Pending transfer",
    "PayeeNote": "Awaiting processing",
    "Reason": {
      "Code": "PAYEE_NOT_FOUND",
      "Message": "Payee not found, transfer pending."
    }
  }
  ```
- **Error Responses**:
  - **400 Bad Request** (Missing orവ`referenceId`):
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
- Set `referenceId` to "success" to trigger a successful transfer response.
- Set `referenceId` to "limitbreached" to trigger `PAYER_LIMIT_REACHED`.
- Set `referenceId` to "insufficient" to trigger `NOT_ENOUGH_FUNDS`.
- Use any other `referenceId` to trigger the default `PENDING` response with `PAYEE_NOT_FOUND`.

---

## 8. Validate Account Holder Status
**Endpoint**: `GET /api/Disbursment/v1_0/accountholder/{accountHolderIdType}/{accountHolderId}/active`  
**Purpose**: Checks if an account holder is active based on their ID type and ID.

### Required Inputs
- **Route Parameters**:
  - `accountHolderIdType`: Type of account holder ID (required, must be "MSISDN", "email", "alias", or "id").
  - `accountHolderId`: Account holder identifier (required).
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (required).
- **Services** (Injected):
  - `AuthContext`: Context for authentication checks.

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
      "Message": "accountHolderIdType must be one of 'MSISDN', 'email', 'alias', or 'id'."
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
  - **400 Bad Request** (Invalid alias/id length):
    ```json
    {
      "Code": "INVALID_ACCOUNT_HOLDER_ID",
      "Message": "accountHolderId is too long for type 'alias' or 'id'."
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
      "Code": słuchINTERNAL_PROCESSING_ERROR",
      "Message": "An internal error occurred while processing."
    }
    ```

### Specific Scenarios
- Omit `accountHolderIdType` or set to an invalid value (e.g., "invalid") to trigger `INVALID_ACCOUNT_HOLDER_ID_TYPE`.
- Omit `accountHolderId` to trigger `INVALID_ACCOUNT_HOLDER_ID`.
- Set `accountHolderIdType` to "MSISDN" and `accountHolderId` to an invalid format (e.g., "123") to trigger `INVALID_MSISDN`.
- Set `accountHolderIdType` to "email" and `accountHolderId` to an invalid format (e.g., "invalid") to trigger `INVALID_EMAIL`.
- Set `accountHolderIdType` to "alias" or "id" and `accountHolderId` to a string longer than 100 characters to trigger `INVALID_ACCOUNT_HOLDER_ID`.
- Set `X-Target-Environment` to "invalid" to trigger `NOT_ALLOWED_TARGET_ENVIRONMENT`.
- Set `accountHolderId` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `accountHolderId` to "inactive" or "notfound" to return `false`.
- Use a valid `accountHolderId` (e.g., "+1234567890" for MSISDN, "test@example.com" for email) to return `true`.

---

This documentation provides a clear guide for developers to test the `DisbursmentController` endpoints, including how to trigger specific success and error responses for presentation or mock-up purposes.