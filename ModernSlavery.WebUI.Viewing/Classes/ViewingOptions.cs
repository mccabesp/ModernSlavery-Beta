﻿using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Viewing.Classes
{
    [Options("Web:Viewing")]
    public class ViewingOptions : IOptions
    {
        /// <summary>
        ///     Specifies how many reporting years the public can view or compare.
        /// </summary>
        public int ShowReportYearCount { get; set; } = 10;

        /// <summary>
        ///     Maximum number of organisations you can add to the compare basket.
        ///     Note: AzureSearch has a limit of 1000 per request.
        /// </summary>
        public int MaxCompareBasketCount { get; set; } = 500;

        /// <summary>
        ///     Maximum number of organisations you can share in a mailto: protocol.
        /// </summary>
        public int MaxCompareBasketShareCount { get; set; } = 195;
    }
}