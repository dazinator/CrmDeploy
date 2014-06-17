CrmDeploy
=========

A .Net Library that makes it simple to deploy plugins / workflows etc to Dynamics CRM programmatically. This is useful for when a manual process such as using the Plugin Registration Tool is not desirable, for example when writing a set of system / integration tests for plugins, or for when your application does not know in advance which entities a particular plugin or component needs to be registered against.

# Deploying Plugins

CrmDeploy is available as a NuGet package: https://www.nuget.org/packages/CrmDeploy/ so it's recommended that you use NuGet to add it to your solution.

The following demonstrates how to deploy a plugin to Dynamics Crm using the fluent API.

```csharp
using CrmDeploy;
using CrmDeploy.Enums;


        public static void Main(string[] args)
        {

            var crmConnectionString =
               @"Url=https://myorg.crm4.dynamics.com/; Username=user@domain.onmicrosoft.com; Password=password; DeviceID=mydevice-dd9f6b7b2e6d; DevicePassword=password";
           
            var deployer = DeploymentBuilder.CreateDeployment()
                                            .ForTheAssemblyContainingThisPlugin<TestPlugin>("Test plugin assembly")
                                            .RunsInSandboxMode()
                                            .RegisterInDatabase()
                                                .HasPlugin<TestPlugin>()
                                                    .WhichExecutesOn(SdkMessageNames.Create, "contact")
                                                    .Synchronously()
                                                    .PostOperation()
                                                    .OnlyOnCrmServer()
                                             .DeployTo(crmConnectionString);

            var registrationInfo = deployer.Deploy();
            if (!RegistrationInfo.Success)
            {
                var reason = registrationInfo.Error.Message;
                Console.WriteLine("Registration failed because: {0}. Rolling deployment back.", reason);
                registrationInfo.Undeploy();
                Console.WriteLine("Deployment was rolled back..");
            }

        }

```

If for any reason the deployment fails, the registrationInfo.Undeploy() method will attempt to roll back the changes that were made.
                

# Multiple Plugin Steps?

You can chain the .AndExecutesOn() methood to register multiple steps for your plugin.

For example, if you want to register your plugin on Create, Update and Delete of a contact you could use the following syntax: 

```csharp

  var deployer = DeploymentBuilder.CreateDeployment()
                                            .ForTheAssemblyContainingThisPlugin<TestPlugin>("Test plugin assembly")
                                            .RunsInSandboxMode()
                                            .RegisterInDatabase()
                                                .HasPlugin<TestPlugin>()
                                                    .WhichExecutesOn(SdkMessageNames.Create, "contact")
                                                    .Synchronously()
                                                    .PostOperation()
                                                    .OnlyOnCrmServer()
                                                    .AndExecutesOn(SdkMessageNames.Update, "contact")
                                                    .Asynchronously()
                                                    .PostOperation()
                                                    .OnCrmServerAndOffline()
                                                    .AndExecutesOn(SdkMessageNames.Delete, "contact")
                                                    .Synchronously()
                                                    .PreOperation()
                                                    .OnlyOffline()
                                             .DeployTo(crmConnectionString.ConnectionString);
```

# Multiple Plugins Steps?

If you want to register more than one plugin, youc an simply chain the .AndHasPlugin() method. For example:-


```csharp

            var deployer = DeploymentBuilder.CreateDeployment()
                                            .ForTheAssemblyContainingThisPlugin<TestPlugin>("Test plugin assembly")
                                            .RunsInSandboxMode()
                                            .RegisterInDatabase()
                                                .HasPlugin<TestPlugin>()
                                                    .WhichExecutesOn(SdkMessageNames.Create, "contact")
                                                    .Synchronously()
                                                    .PostOperation()
                                                    .OnlyOnCrmServer()
                                                 .AndHasPlugin<AnotherTestPlugin>()
                                                    .WhichExecutesOn(SdkMessageNames.Update, "account")
                                                    .Asynchronously()
                                                    .PostOperation()
                                                    .OnCrmServerAndOffline()
                                                  .AndHasPlugin<SomeOtherPlugin>()
                                                    .WhichExecutesOn(SdkMessageNames.Associate, "my_custent", "my_otherent")
                                                    .Synchronously()
                                                    .PreOperation()
                                                    .Rank(2)
                                                    .OnlyOffline()
                                             .DeployTo(crmConnectionString.ConnectionString);

```

