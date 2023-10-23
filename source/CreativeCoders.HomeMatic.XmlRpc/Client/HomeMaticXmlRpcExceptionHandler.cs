using System;
using CreativeCoders.HomeMatic.Core.Exceptions;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Exceptions;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

public class HomeMaticXmlRpcExceptionHandler : IMethodExceptionHandler
{
    public void HandleException(MethodExceptionHandlerArguments arguments)
    {
        if (arguments.MethodException is not FaultException faultException)
        {
            return;
        }

        arguments.MethodException = GetCcuException(faultException);
    }

    private static Exception GetCcuException(FaultException faultException)
    {
        return faultException.FaultCode switch
        {
            -1 => new GeneralException("General exception", faultException),
            -2 => new UnknownDeviceOrChannelException("Device or channel unknown", faultException),
            -3 => new UnknownParamSetException("ParamSet unknown", faultException),
            -4 => new DeviceAddressExpectedException("Device address expected", faultException),
            -5 => new UnknownParameterOrValueException("Parameter or value unknown", faultException),
            -6 => new OperationNotSupportedException("Operation not supported by parameter", faultException),
            -7 => new InterfaceUpdateFailedException("Interface could not make update", faultException),
            -8 => new NotEnoughDutyCycleException("Not enough duty cycles", faultException),
            -9 => new DeviceOutOfReachException("Device out of reach", faultException),
            -10 => new TransmissionOutstandingException("Transmission outstanding", faultException),
            _ => faultException
        };
    }
}