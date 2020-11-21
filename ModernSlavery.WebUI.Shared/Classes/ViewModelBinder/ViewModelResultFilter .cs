using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Interfaces;
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
            var viewResult = context.Result as ViewResult;
            if (viewResult == null || viewResult.Model == null) return;

            //Save the view model to session 
            var viewModelAttribute = viewResult.Model.GetType().GetCustomAttribute<ViewModelAttribute>();
            if (viewModelAttribute == null) return;

            if (viewModelAttribute.StateStore == ViewModelAttribute.StateStores.SessionStash)
                StashModel(context, viewResult.Model);
        }

        private void StashModel(ResultExecutingContext context, object model)
        {
            var controllerType = context.Controller.GetType();
            var modelType = model.GetType();
            var session = context.HttpContext.RequestServices.GetRequiredService<IHttpSession>();
            var keyName = $"{controllerType}:{modelType}:Model";
            session[keyName] = Core.Extensions.Json.SerializeObjectDisposed(model);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
    }
}
