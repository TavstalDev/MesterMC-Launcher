# Security

Purpose
: This file notes what is in place, what is missing, and what to do before running the code in production. The project is provided for learning and experimental use; it is not actively maintained.

Status
- Not actively maintained
- Good for learning and experimentation
- Not ready for production as-is

What is in place
- Hashed passwords and JWT-based API authentication
- Supports multiple authentication schemes (JWT, basic, cookie)
- Uses Entity Framework with parameterized queries to avoid SQL injection
- Basic rate limiting for sensitive endpoints
- Basic security headers and optional HTTPS support

Minimum items to address before production
- Move secrets out of environment files into an encrypted vault you control
- Phone number column is unused, but it should be encrypted if it will be used, some columns of the billing information table should be encrypted
- Add per-user or per-api-key rate limits in addition to IP-based limits
- Add security/audit logging and keep logs separate from primary storage
- Use automated certificate management and avoid plain-text certificate passphrases

Reporting security issues
- Use GitHub's security reporting for this repository rather than public issues

Quick check for vulnerable packages

```bash
cd Api
dotnet list package --vulnerable
```

Use the result to decide which packages to update.