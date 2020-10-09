﻿using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class FilterGroup
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Label { get; set; }
        public bool Expanded { get; set; }
        public string MaxHeight { get; set; }
        public List<OptionSelect> Metadata { get; set; }
        public List<OptionSelect> Selections => Metadata.Where(m => m.Checked).ToList();

    }
}