using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using k8s.Models;

namespace K8sMultiTenantOperator.DataModels
{
    public class MTADeployModel
    {

        [JsonProperty("name")]
        public string DeployName { get; set; }

        [JsonProperty("namespace")]
        public string DeployNamespace { get; set; }

        [JsonProperty("replicas")]
        public int Replicas { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("imagePullPolicy")]
        public string ImagePullPolicy { get; set; }               

        [JsonProperty("ports")]
        public List<int> Ports { get; set; }

        [JsonProperty("annotations")]
        public Dictionary<string, string> Annotations { get; set; }

        [JsonProperty("labels")]
        public Dictionary<string, string> Labels { get; set; }

        [JsonProperty("env")]
        public List<Dictionary<string, string> > Env { get; set; }

        public string ContainerName { get; set; }

        public MTADeployModel()
        {

            Replicas = 1;
            Ports = new List<int>();
            Annotations = new Dictionary<string, string>();
            Labels = new Dictionary<string, string>();
            Env = new List<Dictionary<string, string>>();

        }

        public MTADeployModel(V1Deployment v1Deployment)
        {

            DeployName = v1Deployment.Metadata.Name;
            DeployNamespace = v1Deployment.Metadata.NamespaceProperty;
            Replicas = (int)(v1Deployment.Spec.Replicas);
            Annotations = (Dictionary<string, string>)(v1Deployment.Metadata.Annotations);
            Labels = (Dictionary<string, string>)(v1Deployment.Metadata.Labels);

            Image = v1Deployment.Spec.Template.Spec.Containers[0].Image;
            ImagePullPolicy = v1Deployment.Spec.Template.Spec.Containers[0].ImagePullPolicy;

            var container = v1Deployment.Spec.Template.Spec.Containers[0];
            ContainerName = container.Name;

            foreach (var containerPort in container.Ports)
                Ports.Add(containerPort.ContainerPort);

        }
    }
}
