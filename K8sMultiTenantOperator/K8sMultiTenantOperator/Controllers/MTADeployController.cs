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

    /// <summary>Manages Deployment object in K8s cluster</summary>
    /// <remarks>Inherited from MTAController</remarks>
    public class MTADeployController : MTAController
    {        
        
        private string _deployName;

        /// <summary>MTADeployController - Constructor</summary>
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployName-deploy</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param>
        public MTADeployController(string deployName, string tenantName, string groupName)
            : base(tenantName, groupName)
        {

            _deployName = deployName;

        }

        /// <summary>ReadDeploymentAsync
        /// Fetches a Deployment within from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// Tuple<MTADeployModel, MTAErrorModel>
        /// eithher Error or Deploy model is returned
        /// </returns>
        public async Task<Tuple<MTADeployModel, MTAErrorModel>>
        ReadDeploymentAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var deployParams = PrepareDeployParams(_deployName);

                var v1Deployment = await k8sClient.ReadNamespacedDeploymentAsync
                                                   (deployParams.Item1, namespaceParams);
                var deployModel = new MTADeployModel(v1Deployment);
                return new Tuple<MTADeployModel, MTAErrorModel>(deployModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTADeployModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>CreateDeploymentAsync
        /// Creates a Deployment within the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>
        /// <param name="yamlModel">
        /// V1Deployment model from YAML template file
        /// </param>
        /// <param name="deployModel">
        /// MTADeployModel as sent in Request Body
        /// </param>
        /// <returns>
        /// Tuple<MTADeployModel, MTAErrorModel>
        /// eithher Error or Deploy model is returned
        /// </returns>
        public async Task<Tuple<MTADeployModel, MTAErrorModel>>
        CreateDeploymentAsync(Kubernetes k8sClient, V1Deployment yamlModel,
                              MTADeployModel deployModel)
        {                        

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var deployParams = PrepareDeployParams(_deployName);

                yamlModel.Metadata.Name = deployParams.Item1;
                yamlModel.Metadata.NamespaceProperty = namespaceParams;

                if (deployModel.Annotations != null)
                    yamlModel.Metadata.Annotations = deployModel.Annotations;

                if (deployModel.Labels != null)
                    yamlModel.Metadata.Labels = deployModel.Labels;               

                yamlModel.Spec.Replicas = deployModel.Replicas;
                yamlModel.Spec.Selector.MatchLabels["app"] = deployParams.Item2;
                yamlModel.Spec.Template.Metadata.Labels["app"] = deployParams.Item2;

                var container = yamlModel.Spec.Template.Spec.Containers[0];
                container.Name = deployParams.Item3;
                container.Image = deployModel.Image;
                container.ImagePullPolicy = deployModel.ImagePullPolicy;

                if (deployModel.Resources != null)
                {

                    if (deployModel.Resources.Requests != null)
                    {

                        var cpu = new ResourceQuantity(deployModel.Resources.Requests.CPU);
                        container.Resources.Requests["cpu"] = cpu;

                        var memory = new ResourceQuantity(deployModel.Resources.Requests.Memory);
                        container.Resources.Requests["memory"] = memory;

                    }

                    if (deployModel.Resources.Limits != null)
                    {

                        var cpu = new ResourceQuantity(deployModel.Resources.Limits.CPU);
                        container.Resources.Limits["cpu"] = cpu;

                        var memory = new ResourceQuantity(deployModel.Resources.Limits.Memory);
                        container.Resources.Limits["memory"] = memory;

                    }
                }                

                if (deployModel.Env != null)
                {

                    var v1EnvList = new List<V1EnvVar>();
                    foreach (var env in deployModel.Env)
                    {

                        var v1Env = new V1EnvVar(env["name"], env["value"]);
                        v1EnvList.Add(v1Env);

                    }
                    container.Env = v1EnvList;
                }

                var containerPorts = new List<V1ContainerPort>();
                foreach (var port in deployModel.Ports)
                {

                    var v1ContainerPort = new V1ContainerPort(port);
                    containerPorts.Add(v1ContainerPort);

                }
                container.Ports = containerPorts;

                var v1Deployment = await k8sClient.CreateNamespacedDeploymentAsync
                                                   (yamlModel, namespaceParams);
                deployModel = new MTADeployModel(v1Deployment);
                return new Tuple<MTADeployModel, MTAErrorModel>(deployModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTADeployModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>PatchDeploymentAsync
        /// Patches/Updates a Deployment within the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// <param name="patchModel">
        /// MTADeployModel as sent in Request Body
        /// </param>        
        /// <returns>
        /// Tuple<MTADeployModel, MTAErrorModel>
        /// eithher Error or Deploy model is returned
        /// </returns>
        public async Task<Tuple<MTADeployModel, MTAErrorModel>>
        PatchDeploymentAsync(Kubernetes k8sClient, MTADeployModel patchModel)
        {

            var namespaceParams = PrepareNamespaceParams(_groupName);
            var deployParams = PrepareDeployParams(_deployName);

            var existingDeployment = await k8sClient.ReadNamespacedDeploymentAsync
                                                    (deployParams.Item1, namespaceParams);

            patchModel.Image = existingDeployment.Spec.Template.Spec.Containers[0].Image;
            patchModel.ImagePullPolicy = existingDeployment.Spec.Template.Spec.Containers[0].ImagePullPolicy;
            patchModel.Replicas = (int)(existingDeployment.Spec.Replicas);

            var container = existingDeployment.Spec.Template.Spec.Containers[0];
            container.Name = deployParams.Item3;

            if (patchModel.Env.Count > 0)
            {

                var v1EnvList = new List<V1EnvVar>();
                foreach (var env in patchModel.Env)
                {

                    var v1Env = new V1EnvVar(env["name"], env["value"]);
                    v1EnvList.Add(v1Env);

                }
                container.Env = v1EnvList;
            }

            if (patchModel.Ports.Count > 0)
            {

                var containerPorts = new List<V1ContainerPort>();
                foreach (var port in patchModel.Ports)
                {

                    var v1ContainerPort = new V1ContainerPort(port);
                    containerPorts.Add(v1ContainerPort);

                }
                container.Ports = containerPorts;

            }
            
            try
            {

                
                var v1Patch = new V1Patch(existingDeployment, V1Patch.PatchType.MergePatch);
                var v1Deployment = await k8sClient.PatchNamespacedDeploymentAsync
                                                   (v1Patch, deployParams.Item1, namespaceParams);
                var deployModel = new MTADeployModel(v1Deployment);
                return new Tuple<MTADeployModel, MTAErrorModel>(deployModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTADeployModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;

            }
        }

        /// <summary>DeleteDeploymentAsync
        /// Deletes an existing Deployment from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// MTAErrorModel in case of errors
        /// No Content for successfule completion
        /// eithher Error or No Content is returned
        /// </returns>
        public async Task<MTAErrorModel> DeleteDeploymentAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var deployParams = PrepareDeployParams(_deployName);

                var v1Status = await k8sClient.DeleteNamespacedDeploymentAsync
                                               (deployParams.Item1, namespaceParams);
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
