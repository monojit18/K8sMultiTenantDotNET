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
        public List<PortItem> Ports { get; set; }

        [JsonProperty("selectors")]
        public Dictionary<string, string> Selectors { get; set; }

        public MTAServiceModel()
        {

            Selectors = new Dictionary<string, string>();

        }

        public MTAServiceModel(V1Service v1Service)
        {

            ServiceName = v1Service.Metadata.Name;
            ServiceNamespace = v1Service.Metadata.NamespaceProperty;                        
            Selectors = (Dictionary<string, string>)(v1Service.Spec.Selector);            

        }
    }   

    public class PortItem
    {

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("targetPort")]
        public int TargetPort { get; set; }

    }
}
