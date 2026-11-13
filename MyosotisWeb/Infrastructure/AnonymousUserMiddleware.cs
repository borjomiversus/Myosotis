using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace MyosotisWeb.Infrastructure
{
    public class AnonymousUserMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CookieName = "myosotis_anon_id";

        public AnonymousUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Guid userId;

            if (!context.Request.Cookies.TryGetValue(CookieName, out var rawId) ||
                !Guid.TryParse(rawId, out userId) ||
                userId == Guid.Empty)
            {
                userId = Guid.NewGuid();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                };

                context.Response.Cookies.Append(
                    CookieName,
                    userId.ToString("N"),
                    cookieOptions
                );
            }

            context.Items["AnonUserId"] = userId;

            await _next(context);
        }
    }
}