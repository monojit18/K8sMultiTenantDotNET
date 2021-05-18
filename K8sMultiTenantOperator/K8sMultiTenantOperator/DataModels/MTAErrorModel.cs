using System;
using System.Net;
using Microsoft.Rest;

namespace K8sMultiTenantOperator.DataModels
{
    public class MTAErrorModel
    {

        public string Reason { get; set; }
        public string Message { get; set; }
        public HttpStatusCode Status { get; set; }

        public MTAErrorModel(HttpOperationException exception)
        {

            Message = exception.Message;
            Reason = exception.Response.ReasonPhrase;
            Status = exception.Response.StatusCode;

        }
    }
}
