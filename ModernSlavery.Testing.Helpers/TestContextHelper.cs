using ModernSlavery.Core.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ModernSlavery.Testing.Helpers
{
    public static class TestContextHelper
    {
        public static string GetTestContextName(string defaultValue=null)
        {
            var currentMethod = MethodBase.GetCurrentMethod();

            //Try and get the test name
            string testContextName = null;
            var parentMethod = currentMethod.FindParentMethodWithAttribute<TestAttribute>();
            if (parentMethod != null) testContextName = $"[Test]: {parentMethod.DeclaringType.FullName}.{parentMethod.Name}";

            //Try and get the fixture name
            string testFixtureName=null;
            if (string.IsNullOrWhiteSpace(testContextName))
            {
                var testFixtureMethod = currentMethod.FindParentMethodWithAttribute<TestFixtureAttribute>();
                if (testFixtureMethod != null)
                {
                    testContextName = testFixtureName = testFixtureMethod?.GetCustomAttribute<TestFixtureAttribute>()?.TestName;
                    if (string.IsNullOrWhiteSpace(testContextName))
                        testContextName= testFixtureName = $"[TestFixture]: {testFixtureMethod.DeclaringType.FullName}";
                }
            }

            //Try and get the one time setup name
            if (string.IsNullOrWhiteSpace(testContextName))
            {
                parentMethod = currentMethod.FindParentMethodWithAttribute<OneTimeSetUpAttribute>();
                if (parentMethod != null)
                {
                    if (!string.IsNullOrWhiteSpace(testFixtureName))
                        testContextName = $"[OneTimeSetUp]: {testFixtureName}";
                    else 
                        testContextName = $"[OneTimeSetUp]: {parentMethod.DeclaringType.FullName}";
                }
            }

            if (string.IsNullOrWhiteSpace(testContextName))
            {
                parentMethod = currentMethod.FindParentMethodWithAttribute<OneTimeTearDownAttribute>();
                if (parentMethod != null)
                {
                    if (!string.IsNullOrWhiteSpace(testFixtureName))
                        testContextName = $"[OneTimeTearDown]: {testFixtureName}";
                    else
                        testContextName = $"[OneTimeTearDown]: {parentMethod.DeclaringType.FullName}";
                }
            }
            //Try and get the one time setup name
            if (string.IsNullOrWhiteSpace(testContextName))
            {
                parentMethod = currentMethod.FindParentMethodWithAttribute<SetUpAttribute>();
                if (parentMethod != null)
                {
                    if (!string.IsNullOrWhiteSpace(testFixtureName))
                        testContextName = $"[SetUp]: {testFixtureName}";
                    else
                        testContextName = $"[SetUp]: {parentMethod.DeclaringType.FullName}";
                }
            }

            //Try and get the one time setup name
            if (string.IsNullOrWhiteSpace(testContextName))
            {
                parentMethod = currentMethod.FindParentMethodWithAttribute<TearDownAttribute>();
                if (parentMethod != null)
                {
                    if (!string.IsNullOrWhiteSpace(testFixtureName))
                        testContextName = $"[TearDown]: {testFixtureName}";
                    else
                        testContextName = $"[TearDown]: {parentMethod.DeclaringType.FullName}";
                }
            }
            if (string.IsNullOrWhiteSpace(testContextName))
            {
                parentMethod = currentMethod.FindParentMethodWithAttribute<SetUpFixtureAttribute>();
                if (parentMethod != null)
                {
                    if (!string.IsNullOrWhiteSpace(testFixtureName))
                        testContextName = $"[SetupFixture]: {testFixtureName}";
                    else
                        testContextName = $"[SetupFixture]: {parentMethod.DeclaringType.FullName}";
                }
            }

            if (!string.IsNullOrWhiteSpace(testContextName))return testContextName;

            return defaultValue;
        }
    }
}
