using Microsoft.AspNetCore.Builder;
using MilkiDrugStore.Api.Middlewares;

namespace MilkiDrugStore.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}
