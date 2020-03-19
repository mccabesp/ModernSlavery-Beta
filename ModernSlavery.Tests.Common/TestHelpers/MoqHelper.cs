using System.Linq;
using Moq;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public static class MoqHelper
    {
        /// <summary>
        ///     Returns the mock of a mocked object
        /// </summary>
        /// <typeparam name="T">The type of the mocked object</typeparam>
        /// <param name="mockedObject">The actual mocked object itself</param>
        /// <returns>The mock of the object</returns>
        public static Mock<T> GetMockFromObject<T>(this T mockedObject) where T : class
        {
            var pis = mockedObject.GetType().GetProperties().Where(p => p.PropertyType.Name == "Mock`1").ToArray();
            return pis.First().GetGetMethod().Invoke(mockedObject, null) as Mock<T>;
        }
    }
}