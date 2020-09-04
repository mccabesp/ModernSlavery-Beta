﻿using ModernSlavery.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ModernSlavery.Core.Options
{
    [Options("Email")]
    public class EmailOptions : IOptions
    {
        public string AdminDistributionList { get; set; }
        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void Validate() 
        {
            var exceptions = new List<Exception>();
            if (string.IsNullOrWhiteSpace(AdminDistributionList)) exceptions.Add(new ConfigurationErrorsException("Email:AdminDistributionList cannot be empty"));
            if (Templates.Count==0) exceptions.Add(new ConfigurationErrorsException("Email:Templates cannot be empty"));

        }
    }
}