using System.Collections.Generic;

namespace CrmDeploy
{
    public class ComponentRegistration
    {

        public ComponentRegistration()
        {
            this.PluginAssemblyRegistrations = new List<PluginAssemblyRegistration>();
        }

        public List<PluginAssemblyRegistration> PluginAssemblyRegistrations { get; set; }


    }
}