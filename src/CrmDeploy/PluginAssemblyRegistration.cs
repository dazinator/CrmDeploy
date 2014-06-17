using System.Collections.Generic;
using System.Reflection;
using CrmDeploy.Entities;

namespace CrmDeploy
{
    public class PluginAssemblyRegistration
    {

        public PluginAssemblyRegistration()
        {
            this.PluginTypeRegistrations = new List<PluginTypeRegistration>();
        }

        public ComponentRegistration ComponentRegistration { get; set; }

        public PluginAssembly PluginAssembly { get; set; }
        public Assembly Assembly { get; set; }

        public List<PluginTypeRegistration> PluginTypeRegistrations { get; set; }

    }
}