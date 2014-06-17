CrmDeploy
=========

A .Net Library that makes it simple to deploy plugins / workflows etc to Dynamics CRM programmatically - i.e No need to use a manaul process such as the Plugin Registration Tool. 

# Deploying a Plugin

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
                deployer.Undeploy(RegistrationInfo);
                Console.WriteLine("Deployment was rolled back..");
            }

        }

```
