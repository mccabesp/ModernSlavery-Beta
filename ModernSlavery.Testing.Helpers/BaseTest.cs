using ModernSlavery.Core.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ModernSlavery.Testing.Helpers
{
    public class BaseTest
    {
        public string GetTestMethodName()
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;

            if (string.IsNullOrWhiteSpace(testName)) testName = MethodBase.GetCurrentMethod().FindParentMethodWithAttribute<TestAttribute>().Name;

            return testName;
        }

        public string GetTestFixtureName()
        {
            //Get the method name of the unit test or the parent
            string fixtureName = TestContext.CurrentContext.Test.FullName;

            if (string.IsNullOrWhiteSpace(fixtureName)) fixtureName = MethodBase.GetCurrentMethod().FindParentMethodWithAttribute<TestFixtureAttribute>().Module.Name;

            return fixtureName;
        }

        public string GetTestClassName()
        {
            //Get the method name of the unit test or the parent
            return TestContext.CurrentContext.Test.ClassName;
        }
    }
}
