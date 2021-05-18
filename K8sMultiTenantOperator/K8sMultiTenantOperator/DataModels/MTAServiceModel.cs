using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using k8s.Models;

namespace K8sMultiTenantOperator.DataModels
{
    public class MTAServiceModel
    {

        [JsonProperty("name")]
        public string ServiceName { get; set; }

        [JsonProperty("namespace")]
        public string ServiceNamespace { get; set; }

        [JsonProperty("ports")]
        public string Ports { get; set; }

        [JsonProperty("selectors")]
        public Dictionary<string, string> Selectors { get; set; }              


        public MTAServiceModel(V1Service v1Service)
        {

            ServiceName = v1Service.Metadata.Name;
            ServiceNamespace = v1Service.Metadata.NamespaceProperty;                        
            Selectors = (Dictionary<string, string>)(v1Service.Spec.Selector);            

        }
    }
}
