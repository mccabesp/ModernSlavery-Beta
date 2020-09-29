using AutoFixture;
using ModernSlavery.WebUI.Shared.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebAPI.Public.Classes
{
    public class ListModelExample<T> : IExamplesProvider<List<T>>
    {
        private readonly IFixture _fixture;

        public ListModelExample(IFixture fixture)
        {
            _fixture= fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        public List<T> GetExamples()
        {
            return new List<T>
            {
                _fixture.Create<T>()
            };
        }
    }
}
