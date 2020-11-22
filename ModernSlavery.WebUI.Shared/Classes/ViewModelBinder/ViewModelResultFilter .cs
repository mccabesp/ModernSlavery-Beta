using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Linq;
using System.Reflection;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    public class ViewModelResultFilter : IResultFilter
    {
        private readonly IObfuscator _obfuscator;
        private readonly IDataProtector _dataProtector;

        public ViewModelResultFilter(IDataProtectionProvider provider, IObfuscator obfuscator)
        {
            _dataProtector = provider.CreateProtector(nameof(SecuredAttribute));
            _obfuscator = obfuscator;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            var viewResult = context.Result as ViewResult;
            if (viewResult == null || viewResult.Model == null) return;

            //Save the view model to session 
            var modelType = viewResult.Model.GetType();
            var viewStateAttribute = modelType.GetCustomAttribute<SessionViewStateAttribute>();
            if (viewStateAttribute != null)
            {
                if (!typeof(BaseViewModel).IsAssignableFrom(modelType)) throw new Exception($"Cannot store ViewState in session field for class '{modelType.Name}' which is not derived from class '{nameof(BaseViewModel)}'");
                StashModel(context, viewResult.Model);
            }
        }

        private void StashModel(ResultExecutedContext context, object model)
        {
            var controllerType = context.Controller.GetType();
            var modelType = model.GetType();
            var session = context.HttpContext.RequestServices.GetRequiredService<IHttpSession>();
            var keyName = $"{controllerType}:{modelType}:Model";
            session[keyName] = Core.Extensions.Json.SerializeObjectDisposed(model);
        }
    }
}
