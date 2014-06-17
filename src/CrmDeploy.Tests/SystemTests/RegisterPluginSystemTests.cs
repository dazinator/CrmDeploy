using System;
using System.Configuration;
using CrmDeploy;
using CrmDeploy.Enums;
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
            var crmConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisationService"];
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
                                                    .Synchronously()
                                                    .PostOperation()
                                                    .OnCrmServerAndOffline()
                                             .DeployTo(crmConnectionString.ConnectionString);

            RegistrationInfo = deployer.Deploy();
            if (!RegistrationInfo.Success)
            {
                var reason = RegistrationInfo.Error.Message;
                Assert.Fail("Registration failed because: {0}.", reason);
                // registrationInfo.Undeploy();
                // Console.WriteLine("Deployment was rolled back..");
            }

            //.AndExecutesOn(SdkMessageNames.Delete, "contact")
            //                                       .Synchronously()
            //                                       .PostOperation()
            //                                       .OnlyOffline()

        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            try
            {
                RegistrationInfo.Undeploy();
                Console.WriteLine("Deployment was rolled back..");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.Fail(e.Message);
            }
          
        }

    }
}



