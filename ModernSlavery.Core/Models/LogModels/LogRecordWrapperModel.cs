using System;
using Newtonsoft.Json;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class LogRecordWrapperModel
    {
        public string ApplicationName { get; set; }
        public string FileName { get; set; }
        public string RecordJson { get; set; }
        
        [JsonIgnore]
        public object Record {
            get 
            {
                return string.IsNullOrWhiteSpace(RecordJson) ? null : JsonConvert.DeserializeObject(RecordJson, new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            set 
            {
                RecordJson = value == null ? null : Extensions.Json.SerializeObject(value, typeNameHandling: TypeNameHandling.All);
            }
        }
    }
}