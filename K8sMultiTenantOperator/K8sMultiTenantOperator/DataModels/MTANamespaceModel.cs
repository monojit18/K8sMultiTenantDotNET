using System;
using Newtonsoft.Json;
using k8s.Models;

namespace K8sMultiTenantOperator.DataModels
{
    public class MTANamespaceModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        public MTANamespaceModel(V1Namespace v1Namespace)
        {

            Name = v1Namespace.Metadata.Name;

        }
    }
}
