using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CrmDeploy.Connection;
using CrmDeploy.Entities;
using CrmDeploy.Enums;
using Microsoft.Xrm.Sdk;

namespace CrmDeploy
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing Crm Plugin Registrations.
    /// </summary>
    public class DeploymentBuilder
    {

        protected ComponentRegistration ComponentRegistration { get; set; }

        protected DeploymentBuilder()
        {
            //  PluginAssembly = pluginAssembly;
            //  AttributeBuilder = new EntityAttributeMetadataBuilder(this);
            ComponentRegistration = new ComponentRegistration();
        }

        internal IRegistrationDeployer DeployTo(string orgConnectionString)
        {
            return new RegistrationDeployer(ComponentRegistration, orgConnectionString);
        }

        internal IRegistrationDeployer DeployTo(ICrmServiceProvider crmServiceProvider)
        {
            return new RegistrationDeployer(ComponentRegistration, crmServiceProvider);
        }

        public static DeploymentBuilder CreateDeployment()
        {
            return new DeploymentBuilder();
        }

        public PluginAssemblyOptionsBuilder ForAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var pluginName = assemblyName.Name;
            string version = assemblyName.Version.ToString();

            string publicKeyToken;
            byte[] publicKeyTokenBytes = assemblyName.GetPublicKeyToken();
            if (null == publicKeyTokenBytes || 0 == publicKeyTokenBytes.Length)
            {
                publicKeyToken = null;
            }
            else
            {
                publicKeyToken = string.Join(string.Empty, publicKeyTokenBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }


            var pluginAssembly = new PluginAssembly()
            {
                PluginAssemblyId = Guid.NewGuid(),
                Name = pluginName,
                IsolationMode = new OptionSetValue()
                {
                    Value = (int)IsolationMode.None
                },
                Culture = "neutral",
                PublicKeyToken = publicKeyToken,
                Version = version
            };
            //PluginAssembly = PluginAssembly;
           // var builder = new ComponentRegistrationBuilder();
            var par = new PluginAssemblyRegistration()
                {
                    Assembly = assembly,
                    PluginAssembly = pluginAssembly,
                    ComponentRegistration = this.ComponentRegistration
                };
            this.ComponentRegistration.PluginAssemblyRegistrations.Add(par);
            return new PluginAssemblyOptionsBuilder(this, par);
        }

        public PluginAssemblyOptionsBuilder ForTheAssemblyContainingThisPlugin<T>(string description = "") where T : IPlugin
        {

            var assembly = Assembly.GetAssembly(typeof(T));
            var assemblyName = assembly.GetName();
            //  var pluginAssemblyPath = Path.GetFullPath(assembly.Location);
            // var publicKeyToken = assembly.GetName().GetPublicKeyToken();
            var pluginName = assemblyName.Name;
            string version = assemblyName.Version.ToString();

            string publicKeyToken;
            byte[] publicKeyTokenBytes = assemblyName.GetPublicKeyToken();
            if (null == publicKeyTokenBytes || 0 == publicKeyTokenBytes.Length)
            {
                publicKeyToken = null;
            }
            else
            {
                publicKeyToken = string.Join(string.Empty, publicKeyTokenBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }

            var pluginAssembly = new PluginAssembly()
            {
                PluginAssemblyId = Guid.NewGuid(),
                Name = pluginName,
                IsolationMode = new OptionSetValue()
                {
                    Value = (int)IsolationMode.None
                },
                Culture = "neutral",
                PublicKeyToken = publicKeyToken,
                Version = version,
                Description = description
            };
            //PluginAssembly = PluginAssembly;
           // var builder = new ComponentRegistrationBuilder();

             var par = new PluginAssemblyRegistration()
                {
                    Assembly = assembly,
                    PluginAssembly = pluginAssembly,
                    ComponentRegistration = this.ComponentRegistration
                };
            this.ComponentRegistration.PluginAssemblyRegistrations.Add(par);
            return new PluginAssemblyOptionsBuilder(this, par);
        }

    }
}