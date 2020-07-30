using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;

namespace ModernSlavery.Core.Classes
{
    public class DependencyContractResolver : DefaultContractResolver
    {
        private readonly IServiceProvider _serviceProvider;
        public DependencyContractResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            contract.DefaultCreator = () => GetService(objectType);

            return contract;
        }

        private object GetService(Type objectType)
        {
            var model = _serviceProvider.GetService(objectType);
            if (model == null) model = ActivatorUtilities.CreateInstance(_serviceProvider, objectType);
            return model;
        }
    }
}
