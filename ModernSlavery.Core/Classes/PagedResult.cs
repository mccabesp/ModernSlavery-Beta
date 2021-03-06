﻿using System;
using System.Collections.Generic;

namespace ModernSlavery.Core.Classes
{
    [Serializable]
    public class PagedResult<T>
    {
        public List<T> Results { get; set; } = new List<T>();
        public int CurrentPage { get; set; } = 1;

        public long ActualRecordTotal { get; set; }
        public int ActualPageCount => ActualRecordTotal <= 0 || PageSize <= 0
            ? 0
            : (int) Math.Ceiling((double) ActualRecordTotal / PageSize);

        public int PageSize { get; set; }
        public long VirtualRecordTotal { get; set; }
        public int VirtualPageCount => VirtualRecordTotal <= 0 || PageSize <= 0
            ? 0
            : (int)Math.Ceiling((double)VirtualRecordTotal / PageSize);

    }
}