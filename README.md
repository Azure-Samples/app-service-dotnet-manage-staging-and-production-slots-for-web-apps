---
page_type: sample
languages:
- csharp
products:
- azure
services: App-Service
platforms: dotnet
author: yaohaizh
---

# Getting started on managing staging and product slots for Web Apps in C# #

          Azure App Service basic sample for managing web apps.
           - Create 3 web apps in 3 different regions
           - Deploy to all 3 web apps
           - For each of the web apps, create a staging slot
           - For each of the web apps, deploy to staging slot
           - For each of the web apps, auto-swap to production slot is triggered
           - For each of the web apps, swap back (something goes wrong)


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/app-service-dotnet-manage-staging-and-production-slots-for-web-apps.git

    cd app-service-dotnet-manage-staging-and-production-slots-for-web-apps

    dotnet build

    bin\Debug\net452\ManageWebAppSlots.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.