using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using Geeks.Pangolin.Service.DriverService;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Classes
{
    public class BaseUITest : UITest
    {
        protected readonly IHost _testWebHost;

        public BaseUITest(IHost testWebHost, SeleniumWebDriverService seleniumWebDriverService) : base(seleniumWebDriverService)
        {
            _testWebHost = testWebHost ?? throw new ArgumentNullException(nameof(testWebHost));
        }

        public new void Dispose()
        {
            WebDriver.Manage().Cookies.DeleteAllCookies();
            ResetServiceScope();
            base.Dispose();
        }

        private IServiceScope _ServiceScope;
        public IServiceScope ServiceScope
        {
            get
            {
                return _ServiceScope ??= _testWebHost.CreateServiceScope();
            }
        }

        public void ResetServiceScope()
        {
            _ServiceScope?.Dispose();
            _ServiceScope = null;
        }

        public void ClearCookies()
        {            
            WebDriver.Manage().Cookies.DeleteAllCookies();
        }

        //Sets a value to configuration
        public void ReturntoRoot(UIContext ui)
        {
            ui.Goto("/");
        }

        public void DeleteCookiesAndReturnToRoot(UIContext ui)
        {
            ClearCookies();
            ReturntoRoot(ui);
        }

        

    }

}
