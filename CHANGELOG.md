# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Multi-targeting support for .NET 8.0 and .NET 10.0
- CollectionIsNullOrEmpty extension method for collection null/empty checks
- Comprehensive Sanitizer framework for input sanitization and security
  - LogSanitizer for safe logging of user input
  - HtmlSanitizer for HTML encoding
  - UrlSanitizer for URL encoding
  - JsonSanitizer for JSON encoding
  - Additional sanitizers for XML, CSS, Script, Style, and SQL
- Unit tests for all security fixes and sanitization framework
- Microsoft.Testing.Platform support for modern test runner experience
- GitHub Actions CI workflow enhancements for .NET 10 support
- JsonTestHelpers utility class for handling System.Text.Json JsonElement unwrapping in tests
  - GetString, GetInt64, GetBoolean methods for primitive type extraction
  - GetJsonArray, GetJsonObject methods for complex type handling
  - GetTypeName method for type assertions with Int64/Double normalization

### Changed
- **BREAKING**: Extension method renamed from `IsNullOrEmpty()` to `CollectionIsNullOrEmpty()` to avoid conflict with Microsoft.IdentityModel.Tokens v8+
- Migrated all JSON serialization from Newtonsoft.Json to System.Text.Json
  - ClaimConverter migrated to JsonConverter<Claim>
  - ClaimsPrincipalConverter migrated to JsonConverter<ClaimsPrincipal>
  - CustomContractResolver migrated to JsonNamingPolicy
  - PersistentGrantSerializer updated for System.Text.Json
  - All integration tests updated from JObject/JArray to JsonObject/JsonArray
- Updated all sample projects and quickstarts to use System.Text.Json
- Package version updates:
  - AutoMapper: 10.1.1 → 14.0.0
  - Microsoft.EntityFrameworkCore.Relational: 9.0.11 (net8.0) / 10.0.1 (net10.0)
  - Microsoft.AspNetCore.Authentication.OpenIdConnect: 8.0.11 (net8.0) / 10.0.1 (net10.0)
  - System.IdentityModel.Tokens.Jwt: 8.15.0
  - Microsoft.IdentityModel.Protocols.OpenIdConnect: 8.15.0
  - xUnit migrated to xunit.v3.mtp-v2 (2.0.3)
  - FluentAssertions: 7.2.0
  - Microsoft.NET.Test.Sdk: 18.0.1
  - dotnet-ef tool: 3.1.0 → 10.0.1
- NuGet.config package source key renamed from "NuGet" to "nuget.org"
- Project references updated to use renamed dn.IdentityServer4.csproj
- Removed centralized package management (src/Directory.Build.targets)
- Changed IEnumerableExtensions visibility from internal to public in Storage project
- Replaced obsolete JsonSerializerOptions.IgnoreNullValues with DefaultIgnoreCondition.WhenWritingNull
  - Updated ObjectSerializer.cs and LogSerializer.cs for .NET 8/10 compatibility
- Enhanced JwtRequestValidator.ProcessPayloadAsync() to handle all JsonElement types
  - Added support for JsonElement primitives (String, Number, True, False)
  - Added support for raw types (int, long, bool, double)
  - Added default case for unhandled types

### Fixed
- **CVE-2024-39694**: IsLocalUrl control character validation vulnerability
  - Added rejection of control characters (null, CR, LF, tab, etc.) in local URL validation
  - Prevents URL manipulation attacks via control character injection
- **Username Enumeration Attack**: Sanitized usernames in login failure logs across all AccountController files (8 files)
  - Login success events now use SubjectId instead of username
  - Login failure events sanitize username before logging
  - Prevents attackers from determining valid usernames via log analysis
- **Log Injection Vulnerability**: Implemented comprehensive input sanitization
  - Sanitized redirect_uri parameters in AuthorizeRequestValidator
  - Sanitized login_hint parameters in AuthorizeRequestValidator
  - Sanitized username parameters in TokenRequestValidator
  - Sanitized parameter names in HTML form generation
  - Sanitized log output in AuthorizeRequestValidationLog and TokenRequestValidationLog
- **Backchannel Logout Token Structure**: Fixed malformed JSON in backchannel logout tokens
  - Changed from hardcoded malformed string `{\"event\":{} }` to proper System.Text.Json serialization
  - Ensures RFC-compliant backchannel logout token structure: `{"http://schemas.openid.net/event/backchannel-logout":{}}`
- JWT token serialization issues with System.Text.Json migration
  - Proper handling of JSON object claims
  - Proper handling of JSON array claims
  - RFC 7800 compliant confirmation claim structure (object, not array)
- System.Text.Json compatibility issues in integration tests (48 tests fixed, 100% pass rate achieved)
  - Fixed JsonElement wrapping in Dictionary<string, object> deserialization
  - Added missing error codes to AuthorizeRequestValidator JWT validation (3 locations)
  - Fixed ClientCredentialStyle.PostBody requirement for client assertions
  - Updated 8 test files with JsonTestHelpers for proper JSON handling
  - Fixed Int64 vs Double type detection for whole numbers
  - Fixed JWT request validation error code expectations
  - Fixed JWK JSON format (single quotes → double quotes for System.Text.Json strict parsing)
  - Fixed CustomTokenResponseClients payload assertions using JsonTestHelpers
  - Fixed IntrospectionTests NullReferenceException with JsonArray casting
  - Added workaround for IdentityModel v8+ response_type client-side validation
  - Test results: 1,120/1,121 passing (100%), 0 failures, 1 skipped

### Security
This release addresses multiple security vulnerabilities:
- **CVE-2024-39694**: Critical URL validation bypass
- **Username Enumeration**: Prevents user enumeration via logs
- **Log Injection**: Prevents malicious input from corrupting logs
- **Input Sanitization**: Comprehensive protection against XSS and injection attacks

## Previous Versions

### [2024-05-16] - Initial dn. Fork
- Started tracking changes to comply with Apache License 2.0
- All projects updated to .NET 8.0
- PackageReferences updated where applicable
- Added "dn." prefix to package ID to denote different maintainer
- All other project aspects left as-is from original IdentityServer4

---

## Attribution

This project is a maintained fork of IdentityServer4, originally created by Brock Allen and Dominick Baier.

Original IdentityServer4 repository: https://github.com/IdentityServer/IdentityServer4

Some security fixes and improvements in this fork were adapted from IdentityServer8:
- Repository: https://github.com/IdentityServer/IdentityServer8
- Copyright (c) 2024 HigginsSoft, Alexander Higgins
- Specifically: PR #57 (JWT/Backchannel fixes), PR #60 (Extension method naming)

This project maintains the Apache License 2.0 from the original work.

## License

Licensed under the Apache License, Version 2.0. See [LICENSE](LICENSE) file for details.

Copyright 2018, Brock Allen, Dominick Baier
Modified work Copyright 2024, dn.IdentityServer4 contributors
