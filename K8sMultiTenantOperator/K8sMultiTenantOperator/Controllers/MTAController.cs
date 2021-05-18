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

        protected string _groupName;

        protected string PrepareNamespaceParams(string groupName)

        {

            var namespaceNameString = string.Concat(groupName, kNamespaceTokenString);
            return namespaceNameString;

        }      

        protected Tuple<string, string, string> PrepareDeployParams(string deployName)

        {

            var deployNameString = string.Concat(deployName, kDeployTokenString);
            var podNameString = string.Concat(deployName, kPodTokenString);
            var containerNameString = string.Concat(deployName, kContainerTokenString);

            return new Tuple<string, string, string>
                       (deployNameString, podNameString, containerNameString);
        }

        protected Tuple<string, string> PrepareHPAParams(string hpaName, string deployName)

        {

            var hpaNameString = string.Concat(hpaName, kHPATokenString);
            var deployNameString = string.Concat(deployName, kDeployTokenString);

            return new Tuple<string, string>(deployNameString, hpaNameString);

        }

        protected string PrepareServiceParams(string serviceName)

        {
            
            var serviceNameString = string.Concat(serviceName, kServiceTokenString);
            return serviceNameString;

        }

        public MTAController(string groupName)
        {

            _groupName = groupName;

        }
        
    }
}
