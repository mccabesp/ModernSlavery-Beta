﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly IConfiguration _config; //ms
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next, IConfiguration config, ILogger<ExceptionMiddleware> logger)
        {
            _config = config;
            _logger = logger;
            _next = next;
        }

        [DebuggerHidden()]
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{ex.Message}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync($"ERROR {context.Response.StatusCode} Internal Server Error");
        }
    }
}