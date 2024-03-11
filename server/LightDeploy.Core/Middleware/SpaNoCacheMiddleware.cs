using Microsoft.AspNetCore.Builder;

namespace LightDeploy.Core.Middleware;

public static class SpaNoCacheMiddleware
{
    public static void UseSpaNoCacheMiddleware(this WebApplication app)
    {
        app.Use(async (c, next) =>
        {
            
            c.Response.OnStarting(() =>
            {
                // html不缓存
                if(c.Response.ContentType?.Contains("text/html")??false)
                    c.Response.Headers.Add("Cache-Control", "no-store,no-cache,must-revalidate");
                // js css 缓存1年
                else if ((c.Response.ContentType?.Contains("text/javascript")??false)||(c.Response.ContentType?.Contains("text/css")??false))
                    c.Response.Headers.Add("Cache-Control", "public,max-age=31536000");
                return Task.CompletedTask;
            });
            await next();
           
          
            
        });
    }
}