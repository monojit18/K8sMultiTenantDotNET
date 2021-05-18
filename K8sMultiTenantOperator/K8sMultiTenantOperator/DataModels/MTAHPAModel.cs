using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using k8s.Models;

namespace K8sMultiTenantOperator.DataModels
{
    public class MTAHPAModel
    {

        [JsonProperty("name")]
        public string HPAName { get; set; }

        [JsonProperty("namespace")]
        public string HPANamespace { get; set; }

        [JsonProperty("deployment")]
        public string DeploymentName { get; set; }

        [JsonProperty("maxReplicas")]
        public int MaxReplicas { get; set; }

        [JsonProperty("minReplicas")]
        public int MinReplicas { get; set; }

        [JsonProperty("cpu")]
        public int AvgCPU { get; set; }


        public MTAHPAModel(V1HorizontalPodAutoscaler v1HorizontalPodAutoscaler)
        {

            HPAName = v1HorizontalPodAutoscaler.Metadata.Name;
            HPANamespace = v1HorizontalPodAutoscaler.Metadata.NamespaceProperty;
            DeploymentName = v1HorizontalPodAutoscaler.Spec.ScaleTargetRef.Name;
            MaxReplicas = v1HorizontalPodAutoscaler.Spec.MaxReplicas;
            MinReplicas = (int)v1HorizontalPodAutoscaler.Spec.MinReplicas;
            AvgCPU = (int)v1HorizontalPodAutoscaler.Spec.TargetCPUUtilizationPercentage;


        }
    }
}
