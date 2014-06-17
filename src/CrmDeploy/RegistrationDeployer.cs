using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CrmDeploy.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmDeploy
{
    public class RegistrationDeployer : IRegistrationDeployer
    {
        private readonly ComponentRegistration _Registration;
        private readonly ICrmServiceProvider _ServiceProvider;

        public ComponentRegistration Registration { get { return _Registration; } }

        public RegistrationDeployer(ComponentRegistration registration, string orgConnectionString)
            : this(registration, new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig() { OrganisationServiceConnectionString = orgConnectionString }, new CrmClientCredentialsProvider()))
        {
        }

        public RegistrationDeployer(ComponentRegistration registration)
            : this(registration, new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider()))
        {
        }

        public RegistrationDeployer(ComponentRegistration registration, ICrmServiceProvider serviceProvider)
        {
            _Registration = registration;
            _ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the registration in CRM.
        /// </summary>
        /// <returns></returns>
        public RegistrationInfo Deploy()
        {
            var result = new RegistrationInfo(this);

            try
            {
                var pluginHelper = new PluginHelper(_ServiceProvider);
                foreach (var par in _Registration.PluginAssemblyRegistrations)
                {
                    var pa = par.PluginAssembly;
                    var pluginExists = pluginHelper.DoesPluginAssemblyExist(pa.Name);
                    if (!pluginExists.Exists)
                    {
                        // Create new plugin assembly registration.
                        var newRecordId = pluginHelper.RegisterAssembly(pa);
                        pa.PluginAssemblyId = newRecordId;
                        result.RecordChange(pa.LogicalName, newRecordId);
                    }
                    else
                    {
                        pa.PluginAssemblyId = pluginExists.EntityReference.Id;
                        result.RecordChange(pa.LogicalName, pluginExists.EntityReference.Id);
                    }

                    foreach (var ptr in par.PluginTypeRegistrations)
                    {
                        var pluginTypeExists = pluginHelper.DoesPluginTypeExist(ptr.PluginType.TypeName);

                        if (!pluginTypeExists.Exists)
                        {
                            // Create new plugin type registration.
                            var newRecordId = pluginHelper.RegisterType(ptr.PluginType);
                            ptr.PluginType.PluginTypeId = newRecordId;
                            result.RecordChange(ptr.PluginType.LogicalName, newRecordId);
                        }
                        else
                        {
                            ptr.PluginType.PluginTypeId = pluginTypeExists.EntityReference.Id;
                            result.RecordChange(ptr.PluginType.LogicalName, pluginTypeExists.EntityReference.Id);
                        }

                        // for each step
                        foreach (var ps in ptr.PluginStepRegistrations)
                        {
                            // todo: check primary and secondary entity are valid.
                            // check message name is valid.
                            var messageId = pluginHelper.GetMessageId(ps.SdkMessageName);
                            ps.SdkMessageProcessingStep.SdkMessageId = new EntityReference("sdkmessage", messageId);

                            var sdkFilterMessageId = pluginHelper.GetSdkMessageFilterId(ps.PrimaryEntityName,
                                                                                        ps.SecondaryEntityName,
                                                                                        messageId);
                            ps.SdkMessageProcessingStep.SdkMessageFilterId = new EntityReference("sdkmessagefilter", sdkFilterMessageId);

                            var newRecordId = pluginHelper.RegisterStep(ps.SdkMessageProcessingStep);
                            result.RecordChange(ps.SdkMessageProcessingStep.LogicalName, newRecordId);

                        }
                    }
                }
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Error = e;
                result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Deletes any entities related to the registration, removing the registration from CRM.
        /// </summary>
        /// <param name="regisrationInfo"></param>
        public void Undeploy(RegistrationInfo regisrationInfo)
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());

            // clean up in reverse creation order.

            regisrationInfo.RelatedEntities.Reverse();
            DeleteEntities(service, regisrationInfo.RelatedEntities);
        }

        /// <summary>
        /// Ensures test entity is deleted from CRM.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void DeleteEntities(ICrmServiceProvider serviceProvider, IEnumerable<KeyValuePair<string, Guid>> entities)
        {
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                foreach (var entity in entities)
                {
                    try
                    {
                        orgService.Execute(new DeleteRequest()
                        {
                            Target = new EntityReference() { LogicalName = entity.Key, Id = entity.Value }
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e.Message);
                        throw;
                    }
                 
                    
                }
            }
        }

    }
}