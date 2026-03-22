using System;
using CreativeCoders.HomeMatic.Core.Exceptions;
using CreativeCoders.Net.XmlRpc.Definition;
using CreativeCoders.Net.XmlRpc.Exceptions;

namespace CreativeCoders.HomeMatic.XmlRpc.Client;

/// <summary>
/// Translates HomeMatic XML-RPC fault responses into typed .NET exceptions.
/// </summary>
/// <remarks>
/// The HomeMatic CCU returns XML-RPC faults with numeric fault codes. This handler
/// maps those codes to specific exception types so that callers can catch
/// domain-specific exceptions rather than generic <c>FaultException</c> instances.
/// <para>
/// Fault code mapping (as per the HomeMatic XML-RPC specification):
/// <list type="table">
///   <listheader><term>Code</term><description>Exception</description></listheader>
///   <item><term>-1</term><description><see cref="GeneralException"/> – General error.</description></item>
///   <item><term>-2</term><description><see cref="UnknownDeviceOrChannelException"/> – Unknown device or channel address.</description></item>
///   <item><term>-3</term><description><see cref="UnknownParamSetException"/> – Unknown parameter set.</description></item>
///   <item><term>-4</term><description><see cref="DeviceAddressExpectedException"/> – A device address was expected.</description></item>
///   <item><term>-5</term><description><see cref="UnknownParameterOrValueException"/> – Unknown parameter or value key.</description></item>
///   <item><term>-6</term><description><see cref="OperationNotSupportedException"/> – Operation not supported by the parameter.</description></item>
///   <item><term>-7</term><description><see cref="InterfaceUpdateFailedException"/> – The interface could not perform the update.</description></item>
///   <item><term>-8</term><description><see cref="NotEnoughDutyCycleException"/> – Insufficient RF duty cycle available.</description></item>
///   <item><term>-9</term><description><see cref="DeviceOutOfReachException"/> – The device is out of RF range.</description></item>
///   <item><term>-10</term><description><see cref="TransmissionOutstandingException"/> – A transmission is still outstanding.</description></item>
/// </list>
/// </para>
/// </remarks>
public class HomeMaticXmlRpcExceptionHandler : IMethodExceptionHandler
{
    /// <summary>
    /// Inspects <paramref name="arguments"/> and replaces a <c>FaultException</c> with a typed HomeMatic exception.
    /// </summary>
    /// <param name="arguments">The handler arguments containing the exception to inspect and potentially replace.</param>
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