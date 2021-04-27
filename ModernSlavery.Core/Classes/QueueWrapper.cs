using System;
using Newtonsoft.Json;

namespace ModernSlavery.Core.Classes
{
    public class QueueWrapper
    {
        public QueueWrapper(object record)
        {
            Record = record;
        }

        public string RecordJson { get; set; }

        [JsonIgnore]
        public object Record {
            get {
                return string.IsNullOrWhiteSpace(RecordJson) ? null : JsonConvert.DeserializeObject(RecordJson, new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            set {
                RecordJson = value == null ? null : Core.Extensions.Json.SerializeObject(value, typeNameHandling: TypeNameHandling.All);
            }
        }
    }
}