using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace MyosotisWeb.Infrastructure
{
    public class AnonymousUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDataProtector _protector;
        private const string CookieName = "myosotis_anon_id";

        public AnonymousUserMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider)
        {
            _next = next;
            _protector = dataProtectionProvider.CreateProtector(
                UserSessionContext.ProtectionPurpose);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Guid userId = Guid.Empty;

            if (context.Request.Cookies.TryGetValue(CookieName, out var protectedId))
            {
                try
                {
                    var rawId = _protector.Unprotect(protectedId);
                    if (Guid.TryParseExact(rawId, "N", out var parsedId) && parsedId != Guid.Empty)
                    {
                        userId = parsedId;
                    }
                }
                catch (CryptographicException)
                {

                }
            }

            if (userId == Guid.Empty)
            {
                userId = Guid.NewGuid();
                var protectedValue = _protector.Protect(userId.ToString("N"));

                context.Response.Cookies.Append(CookieName, protectedValue, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                });
            }

            context.Items["AnonUserId"] = userId;

            await _next(context);
        }
    }
}