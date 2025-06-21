using Microsoft.AspNetCore.Diagnostics;

namespace ZODs.Api.ExceptionHandling
{
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Configures the exception handler.
        /// </summary>
        /// <param name="app">Application builder.</param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var exceptionHandler = new BaseExceptionHandler();
                        var errorDetails = exceptionHandler.Handle(contextFeature.Error);
                        context.Response.StatusCode = errorDetails.StatusCode;

                        await context.Response.WriteAsync(errorDetails.ToString());
                    }
                });
            });
        }
    }
}
