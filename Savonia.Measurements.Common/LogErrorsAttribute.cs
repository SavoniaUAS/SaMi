using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Common
{
    public class LogErrorsAttribute : Attribute, IServiceBehavior, IOperationBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var operation in serviceHostBase.Description.Endpoints.SelectMany(endpoint => endpoint.Contract.Operations))
            {
                if (!operation.Behaviors.Any(b => b is LogErrorsAttribute))
                    operation.Behaviors.Add(this);
            }
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new ErrorLoggingOperationInvokerFacade(dispatchOperation.Invoker);
        }

        public void Validate(OperationDescription operationDescription) { }
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation) { }
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters) { }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
    }
}
