# K8sMultiTenantDotNET

## Steps

- This is buolt with .net 5.0
- This will by default launch a server at http://localhost:7070
- Swagger is available at  http://localhost:7070/swagger
- **Controllers**
  - **K8sController** - Contains Http routes; then delegates to appropriate Controllers (*one of the below ones*) to perform actual actions
  - **MTANamespaceController**
  - **MTADeployController**
  - **MTAServiceController**
  - **MTAHPAController**
- Order of calling
  - Create Namespace
  - Create Deployment
  - Create Service
  - Create HPA