using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.Infrastructure.Hosts.WebHost
{
    public static partial class Extensions
    {
        public static IHostBuilder ConfigureWebHostBuilder<TStartup>(this IHostBuilder hostBuilder,
            string contentRoot = null, string webRoot = null) where TStartup : IStartup
        {
            //Build the configuration settings
            var config = Hosts.Extensions.BuildConfiguration();

            //Setup Threads
            config.SetupThreads();

            //Setup SeriLogger
            config.SetupSerilogLogger();

            //Configure the host defaults
            return hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseConfiguration(config);
                webHostBuilder.CaptureStartupErrors(true);
                webHostBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey,
                    "true"); //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                //webHostBuilder.UseEnvironment(configBuilder.EnvironmentName);

                webHostBuilder.ConfigureKestrel(
                    options =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });

                if (!string.IsNullOrWhiteSpace(contentRoot))
                    webHostBuilder.UseContentRoot(contentRoot); //Specify the root path of the content

                if (!string.IsNullOrWhiteSpace(webRoot))
                    webHostBuilder.UseWebRoot(webRoot); //Specify the root path of the site

                //Load the services from the startup class
                webHostBuilder.UseStartup(typeof(TStartup));

                //Add the logging to the web host
                webHostBuilder.ConfigureLogging(
                    builder =>
                    {
                        //For more info see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2
                        builder.ClearProviders();
                        builder.AddConfiguration(config.GetSection("Logging"));
                        builder.AddDebug();
                        builder.AddConsole();
                        builder.AddEventSourceLogger(); //Log to windows event log
                        //builder.AddAzureWebAppDiagnostics(); //Log to live azure stream
                        /* Temporarily removed this line and nuget as its still in beta and was causing a downgrade of AppInsights down to 2.8.1
                        To add nuget again see microsoft.extensions.logging.applicationinsights
                        builder.AddApplicationInsights(); 
                        */
                    });
            });
        }
    }
}