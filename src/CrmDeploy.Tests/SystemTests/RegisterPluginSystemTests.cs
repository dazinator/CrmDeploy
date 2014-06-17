using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using CrmSync.Connection;
using CrmSync.Enums;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using NUnit.Framework;

namespace CrmSync.Tests.SystemTests
{
    [Category("System")]
    [TestFixture]
    public class RegisterPluginSystemTests
    {

        public RegistrationInfo RegistrationInfo = null;

        public RegisterPluginSystemTests()
        {

        }

        [TestFixtureSetUp]
        public void Setup()
        {

        }

        [Test]
        public void RegisterPlugin()
        {
            var serviceProvider = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 
            var deployer = DeploymentBuilder.CreateRegistration()
                                                           .ForTheAssemblyContainingThisPlugin<TestPlugin>()
                                                            .Described("Test plugin assembly")
                                                            .RunsInSandboxMode()
                                                            .LocatedInDatabase()
                                                           .HasPlugin<TestPlugin>()
                                                            .ExecutesOn(SdkMessageNames.Create, "contact")
                                                            .Synchronously()
                                                            .PostOperation()
                                                            .OnlyOnServer()
                                                           .DeployTo(serviceProvider);

            RegistrationInfo = deployer.Deploy();
            if (!RegistrationInfo.Success)
            {
                Assert.Fail("Registration failed..");
                //deployer.Undeploy(updateInfo);
                //Console.WriteLine("Registration was rolled back..");
            }


        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            RegistrationInfo.Undeploy();
        }

    }
}



