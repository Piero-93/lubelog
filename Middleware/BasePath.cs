namespace CarCareTracker.Middleware
{
    /// <summary>
    /// Allows LubeLogger to be served under a sub-path (e.g. https://example.com/lubelogger)
    /// behind a reverse proxy that forwards the full path, prefix included.
    /// Set LUBELOGGER_BASE_PATH=/lubelogger to enable it. Requests without the prefix keep working.
    /// </summary>
    public static class BasePath
    {
        public static string Normalize(string? basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return string.Empty;
            }
            var normalized = "/" + basePath.Trim().Trim('/');
            return normalized == "/" ? string.Empty : normalized;
        }
        /// <summary>
        /// Prepends the base path to an absolute local path ("/Home"), leaving everything else untouched:
        /// full URLs, protocol-relative URLs ("//host"), relative paths and paths that already have the prefix.
        /// </summary>
        public static string Apply(string pathBase, string url)
        {
            if (string.IsNullOrEmpty(pathBase) || string.IsNullOrEmpty(url))
            {
                return url;
            }
            if (!url.StartsWith('/') || url.StartsWith("//"))
            {
                return url;
            }
            if (url == pathBase || url.StartsWith(pathBase + "/") || url.StartsWith(pathBase + "?"))
            {
                return url;
            }
            return pathBase + url;
        }
        public static WebApplication UseBasePath(this WebApplication app, string? configuredBasePath)
        {
            var basePath = Normalize(configuredBasePath);
            if (string.IsNullOrEmpty(basePath))
            {
                return app;
            }
            Console.WriteLine($"Base Path: {basePath}");
            app.UsePathBase(basePath);
            //redirects issued with absolute local paths ("/Login") need the prefix too.
            app.Use(async (context, next) =>
            {
                //read it now: UsePathBase restores the original PathBase before the response starts.
                var pathBase = context.Request.PathBase.Value ?? string.Empty;
                context.Response.OnStarting(() =>
                {
                    var location = context.Response.Headers.Location.ToString();
                    if (!string.IsNullOrEmpty(location))
                    {
                        context.Response.Headers.Location = Apply(pathBase, location);
                    }
                    return Task.CompletedTask;
                });
                await next(context);
            });
            return app;
        }
    }
}
