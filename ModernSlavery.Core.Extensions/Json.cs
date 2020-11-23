using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ModernSlavery.Core.Extensions
{
    public static class Json
    {
        public static string SerializeObject(object value, bool ignoreNulls = true, bool ignoreDefaultValues = true, bool indented = false, TypeNameHandling typeNameHandling=TypeNameHandling.None, ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Error, IContractResolver contractResolver=null)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = referenceLoopHandling,
                NullValueHandling = ignoreNulls ? NullValueHandling.Ignore : NullValueHandling.Include,
                DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                TypeNameHandling = typeNameHandling
            };

            if (contractResolver!=null)jsonSettings.ContractResolver = contractResolver;
            return JsonConvert.SerializeObject(value, indented ? Formatting.Indented : Formatting.None, jsonSettings);
        }
    }
}