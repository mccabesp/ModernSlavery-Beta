using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public static class JsonHelper
    {
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects
        };

        public static T Deserialize<T>(string json) where T : class
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }

        public static string Serialize<T>(T obj) where T : class
        {
            return obj == null ? null : JsonConvert.SerializeObject(obj, JsonSettings);
        }
    }
}
