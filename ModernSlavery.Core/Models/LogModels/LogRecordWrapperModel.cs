using System;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class LogRecordWrapperModel
    {

        public string ApplicationName { get; set; }
        public string FileName { get; set; }
        public object Record { get; set; }

    }
}
