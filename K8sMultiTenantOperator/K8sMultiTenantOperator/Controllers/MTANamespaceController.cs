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

    /// <summary>Manages Namespace object in K8s cluster</summary>
    /// <remarks>Inherited from MTAController</remarks>
    public class MTANamespaceController : MTAController
    {

        /// <summary>MTANamespaceController - Constructor</summary>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param>        
        public MTANamespaceController(string groupName) : base(string.Empty, groupName) { }

        /// <summary>CreateNamespaceAsync
        /// Creates a Namespace within the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// Tuple<MTANamespaceModel, MTAErrorModel>
        /// eithher Error or Namespace model is returned
        /// </returns>
        public async Task<Tuple<MTANamespaceModel, MTAErrorModel>>
        CreateNamespaceAsync(Kubernetes k8sClient)
        {            

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);

                var bodyNamespace = new V1Namespace();
                var metaData = new V1ObjectMeta();
                metaData.Name = namespaceParams;
                bodyNamespace.Metadata = metaData;

                var v1Namespace = await k8sClient.CreateNamespaceAsync(bodyNamespace);
                var nsModel = new MTANamespaceModel(v1Namespace);
                return new Tuple<MTANamespaceModel, MTAErrorModel>(nsModel, null);

            }
            catch (HttpOperationException ex)
            {

                var errorModel = new MTAErrorModel(ex);
                return new Tuple<MTANamespaceModel, MTAErrorModel>(null, errorModel);

            }
            catch (Exception)
            {

                throw;

            }

        }

        /// <summary>DeleteNamespaceAsync
        /// Deletes an existing Namespace from the K8s cluster asynchronously        
        /// </summary>
        /// <param name="k8sClient">
        /// Responsible for connecting to K8s cluster        
        /// </param>        
        /// <returns>
        /// MTAErrorModel in case of errors
        /// No Content for successfule completion
        /// eithher Error or Namespace model is returned
        /// </returns>
        public async Task<MTAErrorModel> DeleteNamespaceAsync(Kubernetes k8sClient)
        {                   

            try
            {

                var namespaceParams = PrepareNamespaceParams(_groupName);
                var v1Status = await k8sClient.DeleteNamespaceAsync(namespaceParams);
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
