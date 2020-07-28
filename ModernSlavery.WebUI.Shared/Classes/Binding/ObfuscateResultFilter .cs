using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.Binding
{
    public class ObfuscatedResultFilter : IResultFilter
    {
        private readonly IObfuscator _obfuscator;

        public ObfuscatedResultFilter(IObfuscator obfuscator)
        {
            _obfuscator=obfuscator;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var viewResult = context.Result as ViewResult;
            if (viewResult == null) return;

            if (!typeof(IEnumerable).IsAssignableFrom(viewResult.Model.GetType()))
                return;

            var model = viewResult.Model as IList;
            foreach (var item in model)
            {
                foreach (var prop in item.GetType().GetProperties())
                {
                    var attribute =
                        prop.GetCustomAttributes(
                            typeof(IObfuscatedAttribute), false).FirstOrDefault();

                    if (attribute != null)
                    {
                        var value = prop.GetValue(item);
                        var obfuscated = _obfuscator.Obfuscate(value.ToString());
                        prop.SetValue(item, obfuscated);
                    }
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
    }

    public class ObfuscatedResultFilterAttribute : TypeFilterAttribute
    {
        public ObfuscatedResultFilterAttribute()
            : base(typeof(ObfuscatedResultFilter))
        { }
    }
}
