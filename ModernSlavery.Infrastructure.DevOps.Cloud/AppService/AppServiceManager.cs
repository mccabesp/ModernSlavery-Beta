using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Infrastructure.Azure.AppService
{
    public class AppServiceManager
    {
        IAzure _azure;
        private IList<IWebApp> _WebApps;
        public IList<IWebApp> WebApps => _WebApps ??= _azure.AppServices.WebApps.List().ToList();

        private IList<IAppServicePlan> _AppServicePlans;
        public IList<IAppServicePlan> AppServicePlans => _AppServicePlans ??= _azure.AppServices.AppServicePlans.List().ToList();

        public AppServiceManager(IAzure azure)
        {
            _azure = azure ?? throw new ArgumentNullException(nameof(azure));
        }

        public enum PricingTiers
        {
            BasicB1,
            SharedD1,
            FreeF1,
            PremiumP3v2,
            PremiumP2v2,
            PremiumP3,
            PremiumP2,
            PremiumP1v2,
            StandardS3,
            StandardS2,
            StandardS1,
            BasicB3,
            BasicB2,
            PremiumP1
        }

        public IWebApp GetWebApp(string webAppName)
        {
            if (string.IsNullOrWhiteSpace(webAppName)) throw new ArgumentNullException(nameof(webAppName));

            return WebApps.FirstOrDefault(w => w.Name.EqualsI(webAppName));
        }
        public IAppServicePlan GetAppServicePlan(string appServicePlanName)
        {
            return AppServicePlans.FirstOrDefault(w => w.Name.EqualsI(appServicePlanName));
        }

        public PricingTier GetPricingTier(PricingTiers pricingTiers)
        {
            switch (pricingTiers)
            {
                case PricingTiers.BasicB1:
                    return PricingTier.BasicB1;
                case PricingTiers.SharedD1:
                    return PricingTier.SharedD1;
                case PricingTiers.FreeF1:
                    return PricingTier.FreeF1;
                case PricingTiers.PremiumP3v2:
                    return PricingTier.PremiumP3v2;
                case PricingTiers.PremiumP2v2:
                    return PricingTier.PremiumP2v2;
                case PricingTiers.PremiumP3:
                    return PricingTier.PremiumP3;
                case PricingTiers.PremiumP2:
                    return PricingTier.PremiumP2;
                case PricingTiers.PremiumP1v2:
                    return PricingTier.PremiumP1v2;
                case PricingTiers.StandardS3:
                    return PricingTier.StandardS3;
                case PricingTiers.StandardS2:
                    return PricingTier.StandardS2;
                case PricingTiers.StandardS1:
                    return PricingTier.StandardS1;
                case PricingTiers.BasicB3:
                    return PricingTier.BasicB3;
                case PricingTiers.BasicB2:
                    return PricingTier.BasicB2;
                case PricingTiers.PremiumP1:
                    return PricingTier.PremiumP1;
            }
            throw new NotImplementedException();
        }

        #region Start/Stop Control
        public void StartWebApp(string webAppName, string slotName=null) { }

        public void StopWebApp(string webAppName, string slotName=null) { }

        public void RestartWebApp(string webAppName, string slotName=null) { }
        #endregion

        #region Deployment Slots
        public IEnumerable<IDeploymentSlot> ListWebAppSlots(string webAppName) 
        {
            var webApp = GetWebApp(webAppName);
            if (webApp == null) return Enumerable.Empty<IDeploymentSlot>();
            return webApp.DeploymentSlots.List();
        }

        public IDeploymentSlot GetWebAppSlot(string webAppName, string slotName)
        {
            var webApp = GetWebApp(webAppName);
            return GetWebAppSlot(webApp, slotName);
        }

        public IDeploymentSlot GetWebAppSlot(IWebApp webApp, string slotName)
        {
            return webApp.DeploymentSlots.GetByName(slotName);
        }

        public IDeploymentSlot CreateWebAppSlot(string webAppName, string slotName, Dictionary<string,string> appSettings) 
        {
            if (slotName.EqualsI("production")) throw new ArgumentOutOfRangeException(nameof(slotName), "Cannot create a production slot");
            var webApp = GetWebApp(webAppName);
            if (webApp==null) throw new ArgumentException(nameof(webAppName), $"Cannot find App Service '{webAppName}'");
            var slot = GetWebAppSlot(webApp, slotName);
            if (slot!=null) throw new ArgumentException(nameof(webAppName), $"Slot '{slotName}' already exists on App Service {webAppName}");
            slot=webApp.DeploymentSlots.Define(slotName)
                .WithConfigurationFromParent()
                .WithAppSettings(appSettings)
                .WithUserAssignedManagedServiceIdentity()
                .Create();

            return slot;
        }

        public void DeleteWebAppSlot(string webAppName, string slotName) 
        {
            if (slotName.EqualsI("production")) throw new ArgumentOutOfRangeException(nameof(slotName), "Cannot delete production slot");
            var webApp = GetWebApp(webAppName);
            if (webApp == null) throw new ArgumentException($"Cannot find App Service '{webAppName}'");
            webApp.DeploymentSlots.DeleteByName(slotName);
        }

        public void SwapWebAppSlot(string webAppName, string sourceSlotName, string targetSlotName)
        {
            if (sourceSlotName.EqualsI(targetSlotName)) throw new ArgumentException(nameof(targetSlotName), $"Cannot swap same slot '{sourceSlotName}' to '{targetSlotName}'");
            var sourceSlot = GetWebAppSlot(webAppName, sourceSlotName);
            sourceSlot.Swap(targetSlotName);
        }

        #endregion

        #region Application Settings
        public Dictionary<string, string> ListApplicationSettings(string webAppName, string slotName)
        {
            return null;
        }

        public void SetApplicationSettings(string webAppName, string slotName, Dictionary<string, string> applicationSettings) { }

        public string GetApplicationSetting(string webAppName, string slotName, string settingName)
        {
            return null;
        }

        public void SetApplicationSetting(string webAppName, string slotName, string settingName, string settingValue) { }

        public string DeleteApplicationSetting(string webAppName, string slotName, string settingName)
        {
            return null;
        }
        #endregion

        #region Backup/Restore
        #endregion

        #region Vertical scaling (App Service Plans)

        public IAppServicePlan GetAppServicePlan(IWebApp webApp)
        {
            if (webApp == null) throw new ArgumentNullException(nameof(webApp));

            var currentPlan = AppServicePlans.FirstOrDefault(p => p.Id == webApp.AppServicePlanId);
            return currentPlan;
        }

        public void SetAppServicePricingTier(string webAppName, PricingTiers newPricingTier)
        {
            var pricingTier = GetPricingTier(newPricingTier);
            SetAppServicePricingTier(webAppName, pricingTier);
        }

        public void SetAppServicePricingTier(string webAppName, PricingTier newPricingTier)
        {
            var webApp = GetWebApp(webAppName);
            if (webApp == null) throw new ArgumentNullException(nameof(webApp), $"Cannot find app service '{webAppName}'");

            SetAppServicePricingTier(webApp, newPricingTier);
        }

        public void SetAppServicePricingTier(IWebApp webApp, PricingTier newPricingTier)
        {
            if (webApp == null) throw new ArgumentNullException(nameof(webApp));
            if (newPricingTier == null) throw new ArgumentNullException(nameof(newPricingTier));

            var newServicePlan = AppServicePlans.FirstOrDefault(p => p.PricingTier == PricingTier.FreeF1);

            if (newServicePlan == null)
            {
                webApp.Update().WithNewAppServicePlan(newPricingTier).Apply();
                _AppServicePlans = null;
            }
            else
                SetAppServicePlan(webApp, newServicePlan);
        }

        public void SetAppServicePlan(IWebApp webApp, IAppServicePlan newServicePlan)
        {
            if (webApp == null) throw new ArgumentNullException(nameof(webApp));
            if (newServicePlan == null) throw new ArgumentNullException(nameof(newServicePlan));

            if (webApp.AppServicePlanId != newServicePlan.Id) webApp.Update().WithExistingAppServicePlan(newServicePlan).Apply();
        }
        #endregion
    }
}
