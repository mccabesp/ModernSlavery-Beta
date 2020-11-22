using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Models
{
    public class BaseViewModel
    {
        [IgnoreMap]
        public bool SaveViewState=true;
    }
}
