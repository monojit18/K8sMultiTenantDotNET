using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.AspNetCore.Mvc;
using k8s;
using k8s.Models;
using K8sMultiTenantOperator.DataModels;

namespace K8sMultiTenantOperator.Controllers
{

    /// <summary>Manages Service object in K8s cluster</summary>
    /// <remarks>Inherited from MTAController</remarks>
    public class MTAServiceController : MTAController
    {
        
        private string _serviceName;

        /// <summary>MTAServiceController - Constructor</summary>
        /// <param name="serviceName">
        /// Represents Service name within the k8s cluster
        /// The actual Service name will be <c>serviceName-svc</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        public MTAServiceController(string serviceName, string tenantName, string groupName)
            : base(tenantName, groupName)
        {

            _serviceName = serviceName;            

        }

        /// <summary>ReadServiceAsync
        /// Fetches a Service within from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// Tuple<MTAServiceModel, MTAErrorModel>
        /// eithher Error or Deploy model is returned
        /// </returns>
        public async Task<Tuple<MTAServiceModel, MTAErrorModel>>
        ReadServiceAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var serviceParams = PrepareServiceParams(_serviceName);

                var v1Service = await k8sClient.ReadNamespacedServiceAsync
                                                (serviceParams, namespaceParams);
                var serviceModel = new MTAServiceModel(v1Service);
                return new Tuple<MTAServiceModel, MTAErrorModel>(serviceModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTAServiceModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>CreateServiceAsync
        /// Creates a Service within the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>
        /// <param name="yamlModel">
        /// V1Service from YAML template file
        /// </param>
        /// <param name="serviceModel">
        /// MTAServiceModel as sent in Request Body
        /// </param>
        /// <returns>
        /// Tuple<MTAServiceModel, MTAErrorModel>
        /// eithher Error or Service model is returned
        /// </returns>
        public async Task<Tuple<MTAServiceModel, MTAErrorModel>>
        CreateServiceAsync(Kubernetes k8sClient, V1Service yamlModel, MTAServiceModel serviceModel)
        {                        

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var serviceParams = PrepareServiceParams(_serviceName);

                yamlModel.Metadata.Name = serviceParams;
                yamlModel.Spec.Selector = serviceModel.Selectors;

                var v1ServicePorts = new List<V1ServicePort>();
                foreach (var port in serviceModel.Ports)
                {

                    var v1ServicePort = new V1ServicePort(port);
                    v1ServicePorts.Add(v1ServicePort);

                }
                yamlModel.Spec.Ports = v1ServicePorts;
                
                var v1Service = await k8sClient.CreateNamespacedServiceAsync
                                                (yamlModel, namespaceParams);
                serviceModel = new MTAServiceModel(v1Service);
                return new Tuple<MTAServiceModel, MTAErrorModel>(serviceModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTAServiceModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>DeleteServiceAsync
        /// Deletes an existing Service from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// MTAErrorModel in case of errors
        /// No Content for successfule completion
        /// eithher Error or No Content is returned
        /// </returns>
        public async Task<MTAErrorModel> DeleteServiceAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var serviceParams = PrepareServiceParams(_serviceName);

                var v1Status = await k8sClient.DeleteNamespacedServiceAsync
                                               (serviceParams, namespaceParams);
                return null;

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return errorModel;

            }
            catch (Exception)
            {

                throw;

            }
        }
    }
}
