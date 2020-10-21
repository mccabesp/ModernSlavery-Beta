using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using Geeks.Pangolin.Service.DriverService;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void ExpectMulti(params string[] pars)
        {
            var exceptions = new ConcurrentBag<Exception>();
            Parallel.ForEach(pars, par =>
            {
                try
                {
                    Expect(par);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
            if (exceptions.Count>0)
            {
                if (exceptions.Count==1)throw exceptions.First();
                throw new AggregateException(exceptions);

            }

            //var tasks = new List<Task>();
            //tasks.Add(new Task(() => Expect("plplplpl")));
            //tasks.Add(new Task(() => Expect("plplplpl")));
            //tasks.Add(new Task(() => Expect("plplplpl")));
            //tasks.Add(new Task(() => Expect("plplplpl")));
            //tasks.Add(new Task(() => Expect("plplplpl")));
            //Task.WaitAll(tasks.ToArray());

        }


    }

}
