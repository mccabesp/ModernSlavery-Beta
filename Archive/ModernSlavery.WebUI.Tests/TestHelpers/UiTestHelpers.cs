﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using ModernSlavery.BusinessDomain;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Tests;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.Mocks;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewServices;
using ModernSlavery.WebUI.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Admin.Controllers;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Viewing.Classes;
using ModernSlavery.WebUI.Viewing.Presenters;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace ModernSlavery.WebUI.Tests.TestHelpers
{
    public static class UiTestHelper
    {

        private const string Url = "https://localhost/";
        public static IContainer DIContainer;

        public static Uri Uri => new Uri(Url, UriKind.Absolute);

        public static T GetController<T>(long userId = 0, RouteData routeData = null, params object[] dbObjects)
        {
            //Create Inversion of Control container
            DIContainer = BuildContainerIoC(dbObjects);
            
            //Mock UserId as claim
            var claims = new List<Claim>();
            if (userId < 0)
            {
                User user = dbObjects.OfType<User>().FirstOrDefault();
                if (user == null)
                {
                    foreach (object item in dbObjects)
                    {
                        var enumerable = item as IEnumerable<object>;
                        if (enumerable != null)
                        {
                            user = enumerable.OfType<User>().FirstOrDefault();
                            if (user != null)
                            {
                                break;
                            }
                        }
                    }
                }

                userId = user.UserId;
            }


            if (userId != 0)
            {
                claims.Add(new Claim("sub", userId.ToString()));
            }

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId > 0)
            {
                mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());
            }

            //Set the Remote IP Address
            var features = new FeatureCollection();
            features.Set<IHttpConnectionFeature>(new HttpConnectionFeature {RemoteIpAddress = IPAddress.Parse("127.0.0.1")});

            //Mock HttpRequest
            var requestMock = new Mock<HttpRequest>();

            var httpContext = new DefaultHttpContext();
            var requestCookies = httpContext.Request.Cookies.Append(new KeyValuePair<string, string>("cookie_settings", JsonConvert.SerializeObject(
                new CookieSettings
                {
                    GoogleAnalyticsGpg = true,
                    GoogleAnalyticsGovUk = true,
                    ApplicationInsights = true,
                    RememberSettings = true
                }))) as IRequestCookieCollection;


            var requestHeaders = new HeaderDictionary();
            //requestMock.Setup(x => x.GetUri()).Returns(Uri);
            //var requestCookies = new RequestCookieCollection(
            //    new Dictionary<string, string> {
            //        {
            //            "cookie_settings",
            //            JsonConvert.SerializeObject(
            //                new CookieSettings {
            //                    GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, ApplicationInsights = true, RememberSettings = true
            //                })
            //        }
            //    });

            requestMock.SetupGet(x => x.Cookies).Returns(requestCookies);
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);

            //Done:Added the queryString Dictionary property for mock object
            var queryString = new QueryString("?code=abcdefg");
            requestMock.SetupGet(x => x.QueryString).Returns(queryString);

            var query = new QueryCollection();

            //Mock HttpResponse
            var responseMock = new Mock<HttpResponse>();
            var responseHeaders = new HeaderDictionary();
            var responseCookies = new Mock<IResponseCookies>();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseMock.SetupGet(x => x.Headers).Returns(responseHeaders);
            responseMock.SetupGet(x => x.Cookies).Returns(responseCookies.Object);

            //Mock session
            var uri = new UriBuilder(Uri);

            //Mock HttpContext
            var httpContextMock = new Mock<HttpContext>();
            //contextMock.Setup(m => m.Cache).Returns(HttpRuntime.Cache);
            httpContextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Headers).Returns(requestHeaders);
            httpContextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Cookies).Returns(requestCookies);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(queryString);
            httpContextMock.SetupGet(ctx => ctx.Request.Query).Returns(query);
            httpContextMock.SetupGet(ctx => ctx.Request.HttpContext).Returns(httpContextMock.Object);
            //contextMock.SetupGet(ctx => ctx.Request.GetUri()).Returns(Uri);
            //contextMock.Setup(ctx => ctx.GetUserHostAddress()).Returns("127.0.0.1");
            httpContextMock.SetupGet(ctx => ctx.Request.Scheme).Returns(uri.Scheme);
            httpContextMock.SetupGet(ctx => ctx.Request.Host).Returns(new HostString(uri.Host, uri.Port));
            httpContextMock.SetupGet(ctx => ctx.Request.Path).Returns(uri.Path);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(new QueryString(uri.Query));

            httpContextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Cookies).Returns(responseCookies.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Headers).Returns(responseHeaders);
            httpContextMock.SetupGet(ctx => ctx.Response.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Session).Returns(new MockHttpSession());
            httpContextMock.SetupGet(ctx => ctx.Features).Returns(features);

            //Mock the httpcontext to the controllercontext

            //var controllerContextMock = new Mock<ControllerContext>();
            var controllerContextMock = new ControllerContext {HttpContext = httpContextMock.Object, RouteData = routeData};

            if (routeData == null)
            {
                routeData = new RouteData();
            }

            //Mock IHttpContextAccessor
            Mock<IHttpContextAccessor> mockHttpContextAccessor = DIContainer.Resolve<IHttpContextAccessor>().GetMockFromObject();
            mockHttpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);

            //Create and return the controller
            var controller = DIContainer.Resolve<T>();

            if (controller is BaseController baseController)
            {
                baseController.ControllerContext = controllerContextMock;

                var mockTempDataSerializer=new Mock<TempDataSerializer>();

                //Setup temp data
                baseController.TempData = new TempDataDictionary(httpContextMock.Object, new SessionStateTempDataProvider(mockTempDataSerializer.Object));

                //Setup the mockUrlHelper for the controller with the calling action from the Route Data
                if (baseController.RouteData != null
                    && baseController.RouteData.Values.ContainsKey("Action")
                    && !string.IsNullOrWhiteSpace(baseController.RouteData.Values["Action"].ToStringOrNull()))
                {
                    baseController.AddMockUriHelper(uri.ToString());
                }

                return (T) Convert.ChangeType(baseController, typeof(T));
            }

            return controller;
        }

        public static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();

            //Create an in-memory version of the database
            if (dbObjects != null && dbObjects.Length > 0)
            {
                builder.RegisterInMemoryTestDatabase(dbObjects);
            }
            else
            {
                Mock<IDataRepository> mockDataRepo = MoqHelpers.CreateMockDataRepository();
                builder.Register(c => mockDataRepo.Object).As<IDataRepository>().InstancePerLifetimeScope();
            }

            // configuration options
            builder.Register(c => MoqHelpers.CreateIOptionsSnapshotMock(new SubmissionOptions()));
            builder.Register(c => MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()));

            //Create the mock repositories
            // BL Repository
            builder.Register(c => new SystemFileRepository(new StorageOptions())).As<IFileRepository>().SingleInstance();
            builder.Register(c => new MockSearchRepository()).As<ISearchRepository<EmployerSearchModel>>().SingleInstance();
            builder.Register(c => Mock.Of<ISearchRepository<SicCodeSearchModel>>()).As<ISearchRepository<SicCodeSearchModel>>();
            builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();
            builder.RegisterType<RegistrationService>().As<IRegistrationService>().SingleInstance();

            // BL Services
            builder.RegisterInstance(ConfigHelpers.Config);
            builder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>().InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<ScopeBusinessLogic>(
                            false,
                            typeof(ISharedBusinessLogic),
                            typeof(IDataRepository),
                            typeof(ISearchBusinessLogic))
                        .Object)
                .As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<SubmissionBusinessLogic>(false, typeof(ISharedBusinessLogic), typeof(IDataRepository)).Object)
                .As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();

            //
            builder.RegisterType<MockPrivateEmployerRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .SingleInstance();
            builder.RegisterType<MockPublicEmployerRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .SingleInstance();
            builder.Register(g => new MockGovNotify()).As<IGovNotifyAPI>().SingleInstance();
            builder.Register(g => new MockClassQueue(QueueNames.SendEmail)).Keyed<IQueue>(QueueNames.SendEmail).SingleInstance();
            builder.Register(g => new MockClassQueue(QueueNames.ExecuteWebJob)).Keyed<IQueue>(QueueNames.ExecuteWebJob).SingleInstance();

            builder.Register(c => Mock.Of<LogEventQueue>()).SingleInstance();
            builder.Register(c => Mock.Of<LogRecordQueue>()).SingleInstance();

            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // UI Presentation
            builder.Register(
                    c =>
                        c.ResolveAsMock<ScopePresenter>(
                                false,
                                typeof(IScopeBusinessLogic),
                                typeof(IDataRepository),
                                typeof(IOrganisationBusinessLogic))
                            .Object)
                .As<IScopePresenter>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ViewingPresenter>().As<IViewingPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>().InstancePerLifetimeScope();

            builder.RegisterType<ComparePresenter>().As<IComparePresenter>().InstancePerLifetimeScope();
            builder.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>().InstancePerLifetimeScope();

            builder.RegisterInstance(new NullLoggerFactory()).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            builder.RegisterType<AuditLogger>().As<AuditLogger>().SingleInstance();

            builder.Register(c => new Mock<IAdminService>().Object).As<IAdminService>().InstancePerLifetimeScope();
            builder.Register(c => Mock.Of<IObfuscator>()).As<IObfuscator>().SingleInstance();
            builder.Register(c => Mock.Of<IEncryptionHandler>()).As<IEncryptionHandler>().SingleInstance();

            //Register WebTracker
            builder.Register(c => Mock.Of<IWebTracker>()).As<IWebTracker>().InstancePerLifetimeScope();

            //Register all BaseControllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();
            
            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterType<AdminUnconfirmedPinsController>().InstancePerLifetimeScope();

            builder.Register(c => new MockCache()).As<IDistributedCache>().SingleInstance();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.Register(c => Mock.Of<IHttpContextAccessor>()).As<IHttpContextAccessor>().InstancePerLifetimeScope();

            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.Register(c => Mock.Of<IUrlHelper>()).SingleInstance();

            //builder.Register(c => new Mock()).As<IFileRepository>();

            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock.SetupGet(m => m.WebRootPath).Returns(Environment.CurrentDirectory);
            builder.RegisterInstance(environmentMock.Object).As<IWebHostEnvironment>().SingleInstance();

            SetupHelpers.SetupMockLogRecordGlobals(builder);

            // Initialise AutoMapper
            MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                // register all out mapper profiles (classes/mappers/*)
                config.AddMaps(typeof(Program));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            IContainer container = builder.Build();

            return container;
        }

        public static void Bind(this Controller controller, object Model)
        {
            var validationContext = new ValidationContext(Model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(Model, validationContext, validationResults, true);
            foreach (ValidationResult validationResult in validationResults)
            {
                controller.ModelState.AddModelError(string.Join(", ", validationResult.MemberNames), validationResult.ErrorMessage);
            }
        }

        /// <summary>
        ///     Adds a new mock url to the action method of an existing or new Mock URl helper of the controller
        /// </summary>
        /// <param name="controller">The source controller to add the mock setup</param>
        /// <param name="url">The url to be returned by the Action method</param>
        /// <param name="actionName">The name of the action to mock. If null tries to get from Controller.RoutData.</param>
        /// <param name="controllerName">
        ///     The name of the action to mock. If null tries to get from Controller.RoutData, then uses
        ///     the Controller name.
        /// </param>
        public static void AddMockUriHelper(this Controller controller, string url, string actionName = null, string controllerName = null)
        {
            actionName = actionName ?? controller.RouteData?.Values["Action"].ToStringOrNull();
            if (string.IsNullOrWhiteSpace(actionName))
            {
                throw new ArgumentNullException(nameof(actionName));
            }

            controllerName = controllerName ?? controller.RouteData?.Values["Controller"].ToStringOrNull();
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                controllerName = controller.GetType().Name;
            }

            if (string.IsNullOrWhiteSpace(controllerName))
            {
                throw new ArgumentNullException(nameof(controllerName));
            }

            if (controllerName != null)
            {
                controllerName = controllerName.BeforeFirst("Controller");
            }

            var uri = new Uri(url);
            if (uri.Authority.EqualsI(Uri.Authority))
            {
                url = uri.PathAndQuery;
            }

            Mock<IUrlHelper> mockUrlHelper = controller.Url?.GetMockFromObject() ?? new Mock<IUrlHelper>();
            Expression<Func<IUrlHelper, string>> urlSetup = helper => helper.Action(
                It.Is<UrlActionContext>(
                    uac => (string.IsNullOrWhiteSpace(actionName) || uac.Action == actionName)
                           && (string.IsNullOrWhiteSpace(controllerName) || uac.Controller == controllerName || uac.Controller == null)));

            mockUrlHelper.Setup(urlSetup).Returns(url).Verifiable();

            controller.Url = mockUrlHelper.Object;
        }

        public static void AddMockQuerystringValue(this Controller controller, string key, string value)
        {
            //controller.HttpContext.Request.Query.Append(new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>(key,value));
            //Set the querystring
            Mock<HttpContext> mockHttpContext = controller.HttpContext.GetMockFromObject();
            mockHttpContext.SetupGet(ctx => ctx.Request.Query[key]).Returns(value);
        }

        public static void AssertCookieAdded(this Controller controller, string key, string value)
        {
            Mock<IResponseCookies> mockCookies = Mock.Get(controller.HttpContext.Response.Cookies);
            mockCookies.Verify(
                c => c.Append(key, value, It.IsAny<CookieOptions>()),
                Times.Once(),
                $"The cookie '{key}' was not saved with value '{value}'");
        }

        public static void AssertCookieDeleted(this Controller controller, string key)
        {
            Mock<IResponseCookies> mockCookies = Mock.Get(controller.HttpContext.Response.Cookies);
            mockCookies.Verify(c => c.Delete(key), Times.Once(), $"The cookie '{key}' was not deleted");
        }

    }
}
