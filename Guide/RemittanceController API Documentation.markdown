# RemittanceController API Documentation

This document outlines the endpoints in the `RemittanceController` for a mock remittance service API. Each endpoint is described with its purpose, required inputs, expected outputs, and specific scenarios that trigger particular responses (including error codes). This is designed to help developers understand how to interact with the API and simulate responses for testing or presentation purposes.

---

## 1. BC Authorize
**Endpoint**: `POST /api/Remittance/v1_0/bc-authorize`  
**Purpose**: Initiates an authorization request for a business client (BC) to access remittance services.

### Required Inputs
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (e.g., sandbox).
  - `X-Callback-Url` (optional): Callback URL for asynchronous notifications.
- **Body** (Form Data):
  - `Scope`: The scope of access (required, non-empty string).
  - `LoginHint`: User identifier hint (required, non-empty string).
  - `AccessType` (optional): Must be either "online" or "offline" if provided.

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

## 2. Create OAuth2 Token
**Endpoint**: `POST /api/Remittance/oauth2/token`  
**Purpose**: Generates an OAuth2 token for authenticated access to remittance services.

### Required Inputs
- **Headers**:
  - `Authorization`: Authentication header (required).
  - `X-Target-Environment`: Specifies the environment (required).
- **Body** (Form Data):
  - `GrantType`: The grant type for the token request (required, non-empty string).

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
- Ensure `X-Target-Environment` is included, or the `ValidateHeaders` method may throw an internal error (not explicitly handled in the code).

---

## 3. Create Access Token
**Endpoint**: `POST /api/Remittance/token`  
**Purpose**: Generates a JWT access token based on an authorization request ID.

### Required Inputs
- **Headers**:
  - `Authorization`: Contains the `auth_req_id` (required, must be a valid GUID).
- **Services** (Injected):
  - `DataContext`: Database context to query `AuthTokens`.
  - `JwtService`: Service to generate JWT tokens.

### Expected Outputs
- **Success Response** (200 OK):
  ```json
  {
    "access_token": "<JWT_TOKEN>",
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

## 4. Create Cash Transfer
**Endpoint**: `POST /api/Remittance/v2_0/cashtransfer`  
**Purpose**: Initiates a cash transfer to a payee.

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
      "Message": "Currency not supported, set currency to GHS"
    }
    ```

### Specific Scenarios
- Omit `Amount`, `Currency`, `Payee.PartyId`, or `Payee.PartyIdType` to trigger `INVALID_REQUEST`.
- Set `X-Reference-Id` to "duplicate" to trigger `RESOURCE_ALREADY_EXIST`.
- Set `Currency` to anything other than "GHS" (e.g., "USD") to trigger `INVALID_CURRENCY`.

---

## 5. Get Cash Transfer Status
**Endpoint**: `GET /api/Remittance/v2_0/cashtransfer/{referenceId}`  
**Purpose**: Retrieves the status of a cash transfer by reference ID.

### Required Inputs
- **Route Parameter**:
  - `referenceId`: Unique reference ID of the transfer (required).
- **Headers**:
  - `X-Target-Environment`: Specifies the environment (required).

### Expected Outputs
- **Success Response** (200 OK):
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
- **Error Responses**:
  - **400 Bad Request** (Missing `referenceId`):
    ```json
    {
      "Code": "INVALID_REFERENCE_ID",
      "Message": "ReferenceId is required."
    }
    ```
  - **404 Not Found** (`referenceId` = "not-found"):
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
  - **200 OK (Failed - Limit Breached)** (`referenceId` = "limit-breached"):
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
  - **200 OK (Failed - Insufficient Balance)** (`referenceId` = "insufficient-balance"):
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

### Specific Scenarios
- Omit `referenceId` or send an empty string to trigger `INVALID_REFERENCE_ID`.
- Set `referenceId` to "not-found" to trigger `RESOURCE_NOT_FOUND`.
- Set `referenceId` to "error" to trigger `INTERNAL_PROCESSING_ERROR`.
- Set `referenceId` to "limit-breached" to trigger `PAYER_LIMIT_REACHED`.
- Set `referenceId` to "insufficient-balance" to trigger `NOT_ENOUGH_FUNDS`.

---

## 6. Create Transfer
**Endpoint**: `POST /api/Remittance/v1_0/transfer`  
**Purpose**: Initiates a remittance transfer to a payee.

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
  - **409 Conflict** (Duplicate `X-Reference-Id`):
    ```json
    {
      "