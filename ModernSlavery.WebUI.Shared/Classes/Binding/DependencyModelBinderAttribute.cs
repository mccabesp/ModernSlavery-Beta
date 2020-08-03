using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.Binding
{
    public class DependencyModelBinderAttribute: ModelBinderAttribute
    {
        public DependencyModelBinderAttribute():base(typeof(DependencyModelBinder))
        {

        }
    }
}
