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

    /// <summary>Manages HPA object in K8s cluster</summary>
    /// <remarks>Inherited from MTAController</remarks>
    public class MTAHPAController : MTAController
    {
        
        private string _hpaName;
        private string _deployName;

        /// <summary>MTAHPAController - Constructor</summary>
        /// <param name="hpaName">
        /// Represents HPA name within the k8s cluster
        /// The actual HPA name will be <c>hpaName-svc</c>        
        /// </param>
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployName-deploy</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        public MTAHPAController(string hpaName, string deployName, string tenantName, string groupName)
            : base(tenantName, groupName)
        {

            _hpaName = hpaName;
            _deployName = deployName;

        }

        /// <summary>ReadHPAAsync
        /// Fetches a HPA within from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// Tuple<MTAHPAModel, MTAErrorModel>
        /// eithher Error or HPA model is returned
        /// </returns>
        public async Task<Tuple<MTAHPAModel, MTAErrorModel>>
        ReadHPAAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var hpaParams = PrepareHPAParams(_hpaName, _deployName);

                var v1HorizontalPodAutoscaler = await k8sClient
                                                      .ReadNamespacedHorizontalPodAutoscalerAsync
                                                      (hpaParams.Item1, namespaceParams);
                var hpaModel = new MTAHPAModel(v1HorizontalPodAutoscaler);
                return new Tuple<MTAHPAModel, MTAErrorModel>(hpaModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTAHPAModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>CreateHPAAsync
        /// Creates a HPA within the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>
        /// <param name="yamlModel">
        /// V1HorizontalPodAutoscaler from YAML template file
        /// </param>
        /// <param name="hpaModel">
        /// MTAHPAModel as sent in Request Body
        /// </param>
        /// <returns>
        /// Tuple<MTAHPAModel, MTAErrorModel>
        /// eithher Error or Deploy model is returned
        /// </returns>
        public async Task<Tuple<MTAHPAModel, MTAErrorModel>>
        CreateHPAAsync(Kubernetes k8sClient, V1HorizontalPodAutoscaler yamlModel,
                       MTAHPAModel hpaModel)
        {                        

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var hpaParams = PrepareHPAParams(_hpaName, _deployName);

                yamlModel.Metadata.Name = hpaParams.Item1;
                yamlModel.Metadata.NamespaceProperty = namespaceParams;
                yamlModel.Spec.ScaleTargetRef.Name = hpaParams.Item2;
                yamlModel.Spec.MaxReplicas = hpaModel.MaxReplicas;
                yamlModel.Spec.MinReplicas = hpaModel.MinReplicas;
                yamlModel.Spec.TargetCPUUtilizationPercentage = hpaModel.AvgCPU;

                var v1HorizontalPodAutoscaler = await k8sClient
                                                      .CreateNamespacedHorizontalPodAutoscalerAsync
                                                      (yamlModel, namespaceParams);
                hpaModel = new MTAHPAModel(v1HorizontalPodAutoscaler);
                return new Tuple<MTAHPAModel, MTAErrorModel>(hpaModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTAHPAModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>DeleteHPAAsync
        /// Deletes an existing HPA from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// MTAErrorModel in case of errors
        /// No Content for successfule completion
        /// eithher Error or No Content is returned
        /// </returns>
        public async Task<MTAErrorModel> DeleteHPAAsync(Kubernetes k8sClient)
        {

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var hpaParams = PrepareHPAParams(_hpaName, _deployName);

                var v1Status = await k8sClient.DeleteNamespacedHorizontalPodAutoscalerAsync
                                               (hpaParams.Item1, namespaceParams);
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
