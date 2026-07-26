# Threat Model — Secure Employee Management API

**Methodology:** STRIDE
**Scope:** ASP.NET Core Web API (authentication, authorization, employee CRUD), backed by a relational database, deployed behind HTTPS.

## 1. System Overview

A RESTful backend for managing employee records. Access is role-gated: Admin users create/update/delete employees; any authenticated user reads. Authentication uses signed JWT bearer tokens; passwords are stored only as BCrypt hashes.

Two trust boundaries exist. Boundary 1 (client to API) is the primary attack surface: every request is treated as hostile until validated and authorized. Boundary 2 (API to database) is protected by parameterized access.

## 2. Assets

| Asset | Why it matters | Sensitivity |
|---|---|---|
| User credentials (password hashes) | Account takeover if compromised | Critical |
| JWT signing key | Whoever holds it can forge any token | Critical |
| JWT tokens (issued) | Grant access for their lifetime | High |
| Employee PII | Privacy / regulatory exposure | High |
| Role assignments | Determine permissions | High |
| API availability | Business depends on uptime | Medium |
| Internal error detail | Aids attacker reconnaissance | Medium |

## 3. Entry Points

| Entry point | Auth required | Notes |
|---|---|---|
| POST /api/auth/register | No | Public; creates Employee-role users only |
| POST /api/auth/login | No | Public; brute-force target |
| GET /api/employees | Yes (any role) | Reads PII |
| GET /api/employees/{id} | Yes (any role) | Reads a single record |
| POST /api/employees | Yes (Admin) | Creates a record |
| PUT /api/employees/{id} | Yes (Admin) | Modifies a record |
| DELETE /api/employees/{id} | Yes (Admin) | Removes a record |

## 4. STRIDE Threat Analysis

### S - Spoofing (Authentication)
- Impersonate a user without credentials -> JWT bearer auth required; 401 otherwise
- Forge a token to appear as Admin -> Signed with server-only secret; tampering invalidates signature; issuer/audience validated
- Replay an old token -> Short 2h lifetime via exp; ValidateLifetime

### T - Tampering (Integrity)
- Modify role claim to Admin -> Signature validation rejects altered tokens
- SQL injection via input -> EF Core parameterizes all queries
- Set server-owned fields via body -> Input DTOs whitelist fields (prevents mass assignment)
- Man-in-the-middle -> HTTPS/TLS; HSTS in production

### R - Repudiation (Non-repudiation)
- User denies an action -> Server-side logging; unique jti per token; identity claims. Improvement: dedicated audit logging.

### I - Information Disclosure (Confidentiality)
- Passwords exposed in a breach -> Only BCrypt hashes stored (salted, adaptive)
- Internal fields returned to clients -> Output DTOs whitelist returned fields
- Stack traces reveal internals -> Global exception middleware returns generic message; detail logged server-side
- API mapped via docs in production -> OpenAPI exposed only in Development
- Username enumeration via login errors -> Single generic "Invalid credentials" message
- Secrets leaked via source control -> JWT key / admin password in User Secrets, never committed

### D - Denial of Service (Availability)
- Credential brute-forcing -> BCrypt slowness raises cost; rate limiting recommended
- Oversized payloads -> Validation and length limits reject early
- Request flooding -> Reverse proxy / cloud throttling; app-level rate limiting recommended

### E - Elevation of Privilege (Authorization)
- Employee performs Admin-only writes -> [Authorize(Roles="Admin")] returns 403 (verified by test)
- Self-register as Admin -> Registration DTO has no role field; server assigns Employee
- Unauthenticated access -> Controller-level [Authorize] requires valid token
- Authorization bypass via pipeline misorder -> UseAuthentication precedes UseAuthorization

## 5. Residual Risks & Recommended Improvements

1. Rate limiting / lockout on login and register
2. Refresh tokens with rotation and revocation
3. Dedicated audit log of write operations
4. Automated dependency scanning (SCA) in CI
5. Static analysis (SAST) in CI
6. Least-privilege database credentials in production
7. Centralized secret management (cloud key vault)

## 6. Summary

The application defends every STRIDE category with concrete, testable controls. Remaining risks are documented with a prioritized remediation path.