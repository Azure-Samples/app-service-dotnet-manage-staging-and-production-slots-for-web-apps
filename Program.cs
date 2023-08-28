// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManageWebAppSlots
{
    public class Program
    {
        private const string Suffix = ".azurewebsites.net";
        private const string SlotName = "staging";

        /**
         * Azure App Service basic sample for managing web apps.
         *  - Create 3 web apps in 3 different regions
         *  - Deploy to all 3 web apps
         *  - For each of the web apps, create a staging slot
         *  - For each of the web apps, deploy to staging slot
         *  - For each of the web apps, auto-swap to production slot is triggered
         *  - For each of the web apps, swap back (something goes wrong)
         */
        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string rgName = Utilities.CreateRandomName("rg1NEMV_");
            string app1Name = Utilities.CreateRandomName("webapp1-");
            string app2Name = Utilities.CreateRandomName("webapp2-");
            string app3Name = Utilities.CreateRandomName("webapp3-");
            var lro = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup = lro.Value;

            try
            {

                //============================================================
                // Create 3 web apps with 3 new app service plans in different regions

                var app1 =await CreateWebApp(resourceGroup, app1Name, region);
                var app2 =await CreateWebApp(resourceGroup, app2Name, region);
                var app3 =await CreateWebApp(resourceGroup, app3Name, region);

                //============================================================
                // Create a deployment slot under each web app with auto swap

                var slot1 =await CreateSlot(app1, SlotName, region);
                var slot2 =await CreateSlot(app2, SlotName, region);
                var slot3 =await CreateSlot(app3, SlotName, region);

                //============================================================
                // Deploy the staging branch to the slot

                DeployToStaging(app1, slot1);
                DeployToStaging(app2, slot2);
                DeployToStaging(app3, slot3);

                // swap back
                SwapProductionBackToSlot(app1, slot1);
                SwapProductionBackToSlot(app2, slot2);
                SwapProductionBackToSlot(app3, slot3);

            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + rgName);
                    await resourceGroup.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + rgName);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
        
        private static async Task<WebSiteResource> CreateWebApp(ResourceGroupResource resourceGroup, string appName, AzureLocation location)
        {
            var appUrl = appName + Suffix;

            Utilities.Log("Creating web app " + appName + " with master branch...");

            var webSiteCollection = resourceGroup.GetWebSites();
            var webSiteData = new WebSiteData(location)
            {
                SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                {
                    WindowsFxVersion = "PricingTier.StandardS1",
                    NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                    PhpVersion = "PhpVersion.V5_6",
                },

            };
            var webSite_lro = await webSiteCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, appName, webSiteData);
            var webSite = webSite_lro.Value;
            Utilities.Log("Created web app " + webSite.Data.Name);
            Utilities.Print(webSite);

            Utilities.Log("CURLing " + appUrl + "...");
            Utilities.Log(Utilities.CheckAddress("http://" + appUrl));
            return webSite;
        }

        private static async Task<WebSiteSlotResource> CreateSlot(WebSiteResource website, String slotName, AzureLocation location)
        {
            Utilities.Log("Creating a slot " + slotName + " with auto swap turned on...");

            var slotCollection = website.GetWebSiteSlots();
            var slotData = new WebSiteData(location)
            {
            };

            var slot_lro = await slotCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, slotName, slotData);
            var slot = slot_lro.Value;
            Utilities.Log("Created slot " + slot.Data.Name);
            Utilities.Print(slot);
            return slot;
        }

        private static async void DeployToStaging(WebSiteResource website, WebSiteSlotResource slot)
        {
            var slotUrl = website.Data.Name + "-" + slot.Data.Name + Suffix;
            var appUrl = website.Data.Name + Suffix;
            var sourceControl = slot.GetWebSiteSlotSourceControl();
            var sourceControl_lro = await sourceControl.CreateOrUpdateAsync(WaitUntil.Completed, new SiteSourceControlData()
            {
                RepoUri = new Uri("https://github.com/jianghaolu/azure-site-test.git"),
                Branch = "staging"
            });
            Utilities.Log("Deploying staging branch to slot " + slot.Data.Name + "...");


            Utilities.Log("Deployed staging branch to slot " + slot.Data.Name);

            Utilities.Log("CURLing " + slotUrl + "...");
            Utilities.Log(Utilities.CheckAddress("http://" + slotUrl));

            Utilities.Log("CURLing " + appUrl + "...");
            Utilities.Log(Utilities.CheckAddress("http://" + appUrl));
        }

        private static void SwapProductionBackToSlot(WebSiteResource website, WebSiteSlotResource slot)
        {
            var appUrl = website.Data.Name + Suffix;
            Utilities.Log("Manually swap production slot back to  " + slot.Data.Name + "...");

            Utilities.Log("Swapped production slot back to " + slot.Data.Name);

            Utilities.Log("CURLing " + appUrl + "...");
            Utilities.Log(Utilities.CheckAddress("http://" + appUrl));
        }
    }
}
