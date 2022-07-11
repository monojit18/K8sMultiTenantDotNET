using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Azure.Management.ContainerService;
using k8s;
using k8s.Models;
using K8sMultiTenantOperator.DataModels;

namespace K8sMultiTenantOperator.Controllers
{

    [ApiController]
    [Route("/")]
    public class K8sController : ControllerBase
    {

        private const string kYAMLPathString = "/Templates/YAMLs";
        private const string kConfigPathString = "/Templates/config";

        private readonly ILogger<K8sController> _logger;
        private readonly Kubernetes _k8sClient;
        private readonly KubernetesClientConfiguration _k8sConfig;
        private readonly DnsManagementClient _dnsManagementClient1;

        private static string GetTemplatesPath()
        {

            var path = Directory.GetCurrentDirectory();
            path = string.Concat(path, kYAMLPathString);
            return path;

        }

        private Tuple<Kubernetes, KubernetesClientConfiguration> PrepareK8s()
        {

            var path = Directory.GetCurrentDirectory();
            path = string.Concat(path, kConfigPathString);

            var k8sConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(path);
            var k8sClient = new Kubernetes(k8sConfig);
            return new Tuple<Kubernetes, KubernetesClientConfiguration>(k8sClient, k8sConfig); ;

        }

        private async Task<DnsManagementClient> PrepareDNSClientAsync(string tenantId, string clientId, string secret)
        {

            var restCredentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
            var dnsManagementClient = new DnsManagementClient(restCredentials);
            return dnsManagementClient;

        }       

        public K8sController(ILogger<K8sController> logger)
        {

            _logger = logger;
            _k8sClient = PrepareK8s().Item1;
            _k8sConfig = PrepareK8s().Item2;
            _dnsManagementClient1 = PrepareDNSClientAsync();

        }

        /// <summary>CreateNamespaceAsync - Creates Namespace within the K8s cluster asynchronously</summary>        
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param>
        /// <example>http://localhost:7070/groups/{groupName}</example>
        [HttpPut]
        [Route("groups/{groupName}", Name = "CreateNamespace")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNamespaceAsync([FromRoute] string groupName)
        {

            var namespaceController = new MTANamespaceController(groupName);
            var respondeModel = await namespaceController.CreateNamespaceAsync(_k8sClient);

            if (respondeModel.Item2 != null)
                return BadRequest(respondeModel.Item2);

            return Created(Url.RouteUrl("CreateNamespace", new { groupName }), respondeModel.Item1);
            
        }

        /// <summary>DeleteNamespaceAsync - Deletes Namespace from the K8s cluster asynchronously</summary>        
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param>              
        /// <example>http://localhost:7070/groups/{groupName}</example>
        [HttpDelete]
        [Route("groups/{groupName}", Name = "DeleteNamespace")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteNamespaceAsync([FromRoute] string groupName)
        {

            var namespaceController = new MTANamespaceController(groupName);
            var respondeModel = await namespaceController.DeleteNamespaceAsync(_k8sClient);

            if (respondeModel != null)
                return BadRequest(respondeModel);

            return NoContent();

        }

        /// <summary>GetDeploymentAsync - Creates Deploymnet in the K8s cluster asynchronously</summary>                
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployname-tenantName-deploy</c>        
        /// </param>
        /// <param name="tenantName">
        /// Represents Tenant name
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/deploy/{deployName}/groups/{groupName}</example>
        [HttpGet]
        [Route("deploy/{deployName}/tenant/{tenantName}/groups/{groupName}", Name = "CreateDeployment")]
        [ProducesResponseType(typeof(MTADeployModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDeploymentAsync
                                         ([FromRoute] string deployName,
                                          [FromRoute] string tenantName,
                                          [FromRoute] string groupName)
        {
            
            var deployController = new MTADeployController(deployName, tenantName, groupName);
            var respondeModel = await deployController.ReadDeploymentAsync(_k8sClient);

            if (respondeModel.Item2 != null)
                return BadRequest(respondeModel.Item2);

            return Ok(respondeModel.Item1);

        }

        /// <summary>CreateDeploymentAsync - Creates Deploymnet in the K8s cluster asynchronously</summary>        
        /// <param name="deployBody">
        /// Represents Deployment model as sent in the Request body        
        /// </param>
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployname-tenantName-deploy</c>        
        /// </param>
        /// <param name="tenantName">
        /// Represents Tenant name
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/deploy/{deployName}/groups/{groupName}</example>
        [HttpPut]
        [Route("deploy/{deployName}/tenant/{tenantName}/groups/{groupName}", Name = "CreateDeployment")]
        [ProducesResponseType(typeof(MTADeployModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDeploymentAsync
                                         ([FromBody] MTADeployModel deployBody,
                                          [FromRoute] string deployName,
                                          [FromRoute] string tenantName,
                                          [FromRoute] string groupName)
        {

            var path = string.Concat(GetTemplatesPath(), "/template-deploy.yaml");
            var deploymentsList = await Yaml.LoadAllFromFileAsync(path);

            var yamlBody = deploymentsList[0] as V1Deployment;
            var deployController = new MTADeployController(deployName, tenantName, groupName);
            var respondeModel = await deployController.CreateDeploymentAsync
                                                       (_k8sClient, yamlBody, deployBody);

            if (respondeModel.Item2 != null)
                return BadRequest(respondeModel.Item2);

            return Created(Url.RouteUrl("CreateDeployment", new { deployName, groupName }),
                                        respondeModel.Item1);

        }

        /// <summary>PatchDeploymentAsync - Patches Deployment in the K8s cluster asynchronously</summary>        
        /// <param name="deployBody">
        /// Represents Deployment model as sent in the Request body        
        /// </param>
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployname-deploy</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/deploy/{deployName}/groups/{groupName}</example>
        [HttpPatch]
        [Route("deploy/{deployName}/tenant/{tenantName}/groups/{groupName}", Name = "PatchDeployment")]
        [ProducesResponseType(typeof(MTADeployModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PatchDeploymentAsync
                                        ([FromBody] MTADeployModel patchBody,
                                         [FromRoute] string deployName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {

            var deployController = new MTADeployController(deployName, tenantName, groupName);            
            var respondeModel = await deployController.PatchDeploymentAsync(_k8sClient, patchBody);

            if (respondeModel != null)
                return BadRequest(respondeModel);

            return Ok(respondeModel.Item1);

        }

        /// <summary>PatchDeploymentAsync - Deletes Deployment from the K8s cluster asynchronously</summary>        
        /// <param name="deployBody">
        /// Represents Deployment model as sent in the Request body        
        /// </param>
        /// <param name="deployName">
        /// Represents Deployment name within the k8s cluster
        /// The actual Deployment name will be <c>deployname-deploy</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/deploy/{deployName}/groups/{groupName}</example>
        [HttpDelete]
        [Route("deploy/{deployName}/tenant/{tenantName}/groups/{groupName}", Name = "DeleteDeployment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteDeploymentAsync
                                        ([FromRoute] string deployName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {

            var deployController = new MTADeployController(deployName, tenantName, groupName);
            var respondeModel = await deployController.DeleteDeploymentAsync(_k8sClient);

            if (respondeModel != null)
                return BadRequest(respondeModel);

            return NoContent();

        }

        /// <summary>CreateServiceAsync - Creates Service in the K8s cluster asynchronously</summary>        
        /// <param name="serviceBody">
        /// Represents Service model as sent in the Request body        
        /// </param>
        /// <param name="serviceName">
        /// Represents Service name within the k8s cluster
        /// The actual Service name will be <c>serviceName-svc</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/service/{serviceName}/groups/{groupName}</example>
        [HttpPut]
        [Route("service/{serviceName}/tenant/{tenantName}/groups/{groupName}", Name = "CreateService")]
        [ProducesResponseType(typeof(MTAServiceModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateServiceAsync
                                         ([FromBody] MTAServiceModel serviceBody,
                                         [FromRoute] string serviceName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {


            var path = string.Concat(GetTemplatesPath(), "/template-svc.yaml");
            var servicesList = await Yaml.LoadAllFromFileAsync(path);

            var yamlBody = servicesList[0] as V1Service;
            var serviceController = new MTAServiceController(serviceName, tenantName, groupName);
            var respondeModel = await serviceController.CreateServiceAsync
                                                       (_k8sClient, yamlBody, serviceBody);

            if (respondeModel.Item2 != null)
                return BadRequest(respondeModel.Item2);

            return Created(Url.RouteUrl("CreateService", new { serviceName, groupName }),
                                        respondeModel.Item1);

        }

        /// <summary>DeleteServiceAsync - Deletes Service from the K8s cluster asynchronously</summary>                
        /// <param name="serviceName">
        /// Represents Service name within the k8s cluster
        /// The actual Service name will be <c>serviceName-svc</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/service/{serviceName}/groups/{groupName}</example>
        [HttpDelete]
        [Route("service/{serviceName}/tenant/{tenantName}/groups/{groupName}", Name = "DeleteService")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteServiceAsync
                                        ([FromRoute] string serviceName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {

            var serviceController = new MTAServiceController(serviceName, tenantName, groupName);
            var respondeModel = await serviceController.DeleteServiceAsync(_k8sClient);

            if (respondeModel != null)
                return BadRequest(respondeModel);

            return NoContent();

        }

        /// <summary>DeleteServiceAsync - Deletes Service from the K8s cluster asynchronously</summary>
        /// <param name="hpaBody">
        /// Represents HPA sent as Request body
        /// </param>
        /// <param name="hpaName">
        /// Represents HPA name within the k8s cluster
        /// The actual HPA name will be <c>hpaName-hpa</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/hpa/{hpaName}/groups/{groupName}</example>
        [HttpPut]
        [Route("hpa/{hpaName}/tenant/{tenantName}/groups/{groupName}", Name = "CreateHPA")]
        [ProducesResponseType(typeof(MTAHPAModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateHPAAsync
                                         ([FromBody] MTAHPAModel hpaBody,
                                         [FromRoute] string hpaName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {


            var path = string.Concat(GetTemplatesPath(), "/template-hpa.yaml");
            var hpaList = await Yaml.LoadAllFromFileAsync(path);

            var yamlBody = hpaList[0] as V1HorizontalPodAutoscaler;
            var deployName = hpaBody.DeploymentName;
            var hpaController = new MTAHPAController(hpaName, deployName, tenantName, groupName);
            var respondeModel = await hpaController.CreateHPAAsync
                                                    (_k8sClient, yamlBody, hpaBody);

            if (respondeModel.Item2 != null)
                return BadRequest(respondeModel.Item2);

            return Created(Url.RouteUrl("CreateService", new { hpaName, groupName }),
                                        respondeModel.Item1);

        }

        /// <summary>DeleteServiceAsync - Deletes Service from the K8s cluster asynchronously</summary>        
        /// <param name="hpaName">
        /// Represents HPA name within the k8s cluster
        /// The actual HPA name will be <c>hpaName-hpa</c>        
        /// </param>
        /// <param name="groupName">
        /// Represents Namespace name within the k8s cluster
        /// The actual Namespace name will be <c>groupName-ns</c>        
        /// </param> 
        /// <example>http://localhost:7070/hpa/{hpaName}/groups/{groupName}</example>
        [HttpDelete]
        [Route("hpa/{hpaName}/tenant/{tenantName}/groups/{groupName}", Name = "DeleteHPA")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MTAErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteHPAAsync
                                        ([FromRoute] string hpaName,
                                         [FromRoute] string tenantName,
                                         [FromRoute] string groupName)
        {

            var serviceController = new MTAHPAController(hpaName, string.Empty, tenantName, groupName);
            var respondeModel = await serviceController.DeleteHPAAsync(_k8sClient);

            if (respondeModel != null)
                return BadRequest(respondeModel);

            return NoContent();

        }
    }
}
