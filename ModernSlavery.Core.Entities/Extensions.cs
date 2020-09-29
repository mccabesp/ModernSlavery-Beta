using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public static class Extensions
    {
        public static string GetDisplayDescription(this StatementTurnovers? turnover)
        {
            if (turnover == null) turnover = StatementTurnovers.NotProvided;
            return turnover.Value.GetDisplayDescription();
        }
    }
}
