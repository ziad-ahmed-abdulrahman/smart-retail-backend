# SmartRetail API Documentation

This directory contains Postman collections and environment files for testing and integrating the SmartRetail backend services.

## Files
- `SmartRetail-API.postman_collection.json`: Complete API collection containing all endpoints, descriptions, request bodies, query parameters, and auto-token extraction scripts.
- `SmartRetail-Environment.postman_environment.json`: Environment file pre-configured with localhost URL and default credentials.

---

## How to Import into Postman
1. Open **Postman**.
2. Click **Import** (top left).
3. Select both files:
   - `SmartRetail-API.postman_collection.json`
   - `SmartRetail-Environment.postman_environment.json`
4. Select `SmartRetail-Environment` as your active environment in the top right environment dropdown.

---

## Authentication & Role Structure

### Roles
1. **SuperAdmin**: Platform owner who manages the software system and all user accounts.
   - Seeded Email: `superadmin@smartretail.com`
   - Seeded Password: `Password123!`
2. **Owner**: Supermarket owner who manages stores, products, and automated devices.
   - Any new account created via `/api/accounts/register` receives the **Owner** role automatically.
   - Seeded Email: `owner@smartretail.com`
   - Seeded Password: `Password123!`

---

## Endpoints Overview

### 1. Authentication & Registration
- `POST /api/accounts/register`: Register new Owner account. Account is inactive until verified.
- `POST /api/accounts/account-activations`: Send new OTP activation code to email.
- `POST /api/accounts/verify-account-activations`: Verify OTP code and activate account.
- `POST /api/accounts/login`: Authenticate and receive JWT Bearer token + Refresh Token. (Auto-stored in environment variables).
- `POST /api/accounts/refresh-token`: Renew expired JWT using refresh token.
- `POST /api/accounts/revoke-token`: Invalidate refresh token (Logout).

### 2. Password Management
- `POST /api/accounts/account-resets`: Request password reset OTP code.
- `POST /api/accounts/verify-account-resets`: Verify reset OTP code and set new password.
- `POST /api/accounts/change-password-code`: Request password change code (Authenticated).
- `POST /api/accounts/verify-change-password-code`: Verify and update password (Authenticated).

### 3. User Profile & Address
- `GET /api/accounts/me`: Retrieve current authenticated user profile and roles.
- `PUT /api/accounts/me`: Update first name, last name, or phone number.
- `PUT /api/accounts/address`: Create or update address details.

### 4. SuperAdmin Management
- `GET /api/accounts/users?pageIndex=1&pageSize=10`: Retrieve paginated list of all registered users (SuperAdmin only).
- `POST /api/accounts/users/{id}/block`: Block user access (SuperAdmin only).
- `POST /api/accounts/users/{id}/unblock`: Restore user access (SuperAdmin only).
