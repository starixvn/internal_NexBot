namespace NexBot
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using NexBot.Core.Services.Interfaces;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogProviderService logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogProviderService logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogFileError(ex);
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception exception)
        {
            await logger.SentryErrorAsync(exception.ToString(), exception);

            var method = context.Request.Method.ToLowerInvariant();
            var requestedWith = context.Request.Headers["X-Requested-With"];

            if ((method == "post" || method == "put" || method == "delete") && requestedWith == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}