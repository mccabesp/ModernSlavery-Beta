using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    public class CustomSimpleTypeModelProvider: SimpleTypeModelBinder
    {
        public CustomSimpleTypeModelProvider(Type type, ILoggerFactory loggerFactory):base(type,loggerFactory)
        {

        }

        protected override void CheckModel(ModelBindingContext bindingContext, ValueProviderResult valueProviderResult, object model)
        {
            base.CheckModel(bindingContext, valueProviderResult, model);
        }

    }
}
