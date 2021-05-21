using System;
namespace K8sMultiTenantOperator.Controllers
{
    public class MTAController
    {

        protected const string kNamespaceTokenString = "-ns";
        protected const string kDeployTokenString = "-deploy";
        protected const string kPodTokenString = "-pod";
        protected const string kContainerTokenString = "-app";
        protected const string kServiceTokenString = "-svc";
        protected const string kHPATokenString = "-hpa";

        protected string _tenantName;
        protected string _groupName;

        protected string PrepareNamespaceParams(string groupName)
        {

            var namespaceNameString = string.Concat(groupName, kNamespaceTokenString);
            return namespaceNameString;

        }      

        protected Tuple<string, string, string> PrepareDeployParams(string deployName)
        {

            var deployNameString = $"{deployName}-{_tenantName}{kDeployTokenString}";
            var podNameString = $"{deployName}{kDeployTokenString}";
            var containerNameString = $"{deployName}{kContainerTokenString}";

            return new Tuple<string, string, string>
                       (deployNameString, podNameString, containerNameString);
        }

        protected Tuple<string, string> PrepareHPAParams(string hpaName, string deployName)
        {

            var hpaNameString = $"{hpaName}-{_tenantName}{kHPATokenString}";
            var deployNameString = $"{deployName}{kDeployTokenString}";

            return new Tuple<string, string>(deployNameString, hpaNameString);

        }

        protected string PrepareServiceParams(string serviceName)
        {

            var serviceNameString = $"{serviceName}-{_tenantName}{kServiceTokenString}";
            return serviceNameString;

        }

        public MTAController(string tenantName, string groupName)
        {

            _tenantName = tenantName;
            _groupName = groupName;

        }
        
    }
}
