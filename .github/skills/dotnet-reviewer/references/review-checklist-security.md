# Review Checklist — Security

## Input Validation

- All public-API entry points (controllers, minimal-API handlers, gRPC, message handlers) validate input.
- DTOs use `[Required]`, `[Range]`, `[StringLength]`, or FluentValidation rules. Flag DTOs without any validation.
- Reject untrusted input before it reaches the data layer. No ad-hoc string parsing in business logic.

## Injection

- **SQL:** parameterized queries via EF Core LINQ, `FromSqlInterpolated`, or `DbCommand` with parameters. Flag: `FromSqlRaw` with interpolation, raw `SqlCommand` with concatenation.
- **Command/Process:** `Process.Start` with user-controlled args — flag any concatenation; require `ProcessStartInfo` with `ArgumentList`.
- **LDAP/XPath:** sanitized filter construction.
- **Path traversal:** `Path.Combine` does not protect — validate that the resolved absolute path is under the intended root.

## Authentication / Authorization

- `[Authorize]` (or equivalent) on every non-anonymous endpoint. Flag controllers without `[Authorize]` at class level when most actions need auth.
- Role/policy checks match the intent (no `[Authorize(Roles = "Admin")]` for user-scoped data; use resource-based authorization).
- No "fallback to anonymous" branches in middleware.

## Secrets and Credentials

- No secrets in source: API keys, connection strings with passwords, JWT keys.
- `appsettings.*.json` with secrets must be `.gitignore`-d; production uses a secret manager.
- Logs do not include `request.Headers`, `Request.Body`, or full URLs containing tokens.
- `HttpClient` does not blindly trust certs (`ServerCertificateCustomValidationCallback` should not return `true`).

## Deserialization & Serialization

- `JsonSerializer` does not deserialize untrusted JSON to `object` or `dynamic`.
- `BinaryFormatter` is forbidden — flag any usage.
- XML deserialization disables DTD processing (`XmlReaderSettings.DtdProcessing = Prohibit`).

## Cryptography

- No `MD5` or `SHA1` for security purposes. Use `SHA256+`.
- No `RandomNumberGenerator.Create()` followed by predictable seeding — use `RandomNumberGenerator.GetBytes()`.
- AES with `CipherMode.ECB` is forbidden. Prefer authenticated modes (`AesGcm`).
- No hard-coded IVs or keys.

## Web App Specific

- CSRF protection on state-changing endpoints (cookie auth + non-GET).
- Anti-forgery tokens for forms.
- CORS is restrictive — `AllowAnyOrigin` only on read-only public endpoints.
- HSTS, secure cookies, `SameSite=Lax` minimum.
- Output encoding — Razor auto-encodes; `Html.Raw` is a code smell that needs justification.

## OWASP Mapping

When reporting, reference the OWASP Top 10 category in parentheses:
- A01 Broken Access Control
- A02 Cryptographic Failures
- A03 Injection
- A04 Insecure Design
- A05 Security Misconfiguration
- A07 Identification & Auth Failures
- A08 Software & Data Integrity Failures
- A09 Security Logging Failures
- A10 SSRF
