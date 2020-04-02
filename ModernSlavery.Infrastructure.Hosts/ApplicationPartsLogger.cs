using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace ModernSlavery.Infrastructure.Hosts
{
    public class ApplicationPartsLogger : IHostedService
    {
        private readonly ILogger<ApplicationPartsLogger> _logger;
        private readonly ApplicationPartManager _partManager;

        public ApplicationPartsLogger(ILogger<ApplicationPartsLogger> logger, ApplicationPartManager partManager)
        {
            _logger = logger;
            _partManager = partManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Get the names of all the application parts. This is the short assembly name for AssemblyParts
            var applicationPartNames = _partManager.ApplicationParts.Select(x => x.Name).ToList();
    
            // Create a controller feature, and populate it from the application parts
            var controllerFeature = new ControllerFeature();
            _partManager.PopulateFeature(controllerFeature);

            // Get the names of all of the controllers
            var controllerNames = controllerFeature.Controllers.Select(x => x.Name).ToList();

            // Create a view feature, and populate it from the application parts
            var viewComponentFeatures = new ViewComponentFeature();
            _partManager.PopulateFeature(viewComponentFeatures);

            // Get the names of all of the view components
            var viewComponentNames = viewComponentFeatures.ViewComponents.Select(x => x.Name).ToList();

            // Create a razor views feature, and populate it from the application parts
            var razorViewFeatures = new ViewsFeature();
            _partManager.PopulateFeature(razorViewFeatures);

            // Get the names of all of the razor views
            var razorViewsNames = razorViewFeatures.ViewDescriptors.Select(x => x.RelativePath).ToList();

            // Create a view feature, and populate it from the application parts
            var tagHelperFeatures = new TagHelperFeature();
            _partManager.PopulateFeature(tagHelperFeatures);

            // Get the names of all of the views
            var tagHelperNames = tagHelperFeatures.TagHelpers.Select(x => x.Name).ToList();

            // Log the controllers
            if (applicationPartNames.Any()) _logger.LogInformation($"Found the following application parts: '{string.Join(", ", applicationPartNames)}'");

            // Log the controllers
            if (controllerNames.Any()) _logger.LogInformation($"Found the following controllers: '{string.Join(", ", controllerNames)}'");

            // Log the controllers
            if (viewComponentNames.Any())_logger.LogInformation($"Found the following view components: '{string.Join(", ", viewComponentNames)}'");

            // Log the razor views
            if (razorViewsNames.Any()) _logger.LogInformation($"Found the following razor views: '{string.Join(", ", razorViewsNames)}'");

            // Log the tag helpers
            if (tagHelperNames.Any()) _logger.LogInformation($"Found the following tag helpers: '{string.Join(", ", tagHelperNames)}'");

            return Task.CompletedTask;
        }

        // Required by the interface
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
