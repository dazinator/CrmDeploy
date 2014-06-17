using System;
using System.Collections.Generic;

namespace CrmDeploy
{

    public class RegistrationInfo
    {
        private IRegistrationDeployer _Deployer;

        internal RegistrationInfo(IRegistrationDeployer deployer)
        {
            RelatedEntities = new List<KeyValuePair<string, Guid>>();
            _Deployer = deployer;
        }

        public bool Success { get; set; }
        public List<KeyValuePair<string, Guid>> RelatedEntities { get; set; }
        public Exception Error { get; set; }

        public void Undeploy()
        {
            _Deployer.Undeploy(this);
        }

        internal void RecordChange(string entityName, Guid id)
        {
            this.RelatedEntities.Add(new KeyValuePair<string, Guid>(entityName, id));
        }
    }
}