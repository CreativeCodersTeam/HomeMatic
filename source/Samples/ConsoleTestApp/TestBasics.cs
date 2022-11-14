using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using CreativeCoders.Core.Collections;
using CreativeCoders.HomeMatic.Api;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using CreativeCoders.Net;
using CreativeCoders.Net.Servers.Http.AspNetCore;
using CreativeCoders.Net.XmlRpc.Proxy;
using CreativeCoders.Net.XmlRpc.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTestApp;

[SuppressMessage("ReSharper", "UnusedVariable")]
public class TestBasics
{
    private IServiceProvider _serviceProvider;

    public async Task Run()
    {
        _serviceProvider = CreateServiceProvider();

        //var apiBuilder = _serviceProvider.GetRequiredService<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
        
        var apiBuilder = _serviceProvider.GetRequiredService<IHomeMaticXmlRpcApiBuilder>();
            
        var xmlRpcApiHmIp = apiBuilder
            .ForUrl("http://192.168.1.220:" + CcuRpcPorts.HomeMaticIp)
            .Build();

        var xmlRpcApi = apiBuilder
            .ForUrl("http://192.168.1.220:" + CcuRpcPorts.HomeMatic)
            .Build();

        var connection = new CcuConnection(xmlRpcApiHmIp);

        var deviceInfos = await connection.GetDeviceInfosAsync();

        var devices = await connection.GetDevicesAsync();

        //var temperature = await xmlRpcApi.GetValueAsync<double>("NEQ1142873:1", "TEMPERATURE");

        //Console.WriteLine($"Temperature: {temperature}");

        var deviceDescriptions = await xmlRpcApiHmIp.ListDevicesAsync();
            
        deviceDescriptions.ForEach(x => Console.WriteLine($"{x.Address}"));
            
        Console.WriteLine();

        await LoadParameterDescriptions(xmlRpcApiHmIp);
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection() as IServiceCollection;

        services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
        services.AddHomeMaticXmlRpc();
            
        return services.BuildServiceProvider();
    }

    private async Task LoadParameterDescriptions(IHomeMaticXmlRpcApi homeMaticXmlRpcApi)
    {
        Console.WriteLine("Press key to continue...");
        Console.ReadKey();
        Console.WriteLine("Starting xml rpc server...");

        var httpServer = new AspNetCoreHttpServer
        {
            DisableLogging = false,
            AllowSynchronousIO = true
        };

        using var xmlRpcServer = new XmlRpcServer(httpServer, true)
        {
            Encoding = Encoding.GetEncoding("iso-8859-1")
        };
        xmlRpcServer.Urls.Add(XmlRpcUrl);

        var ccuXmlRpcServer = _serviceProvider.GetRequiredService<ICcuXmlRpcEventServerFactory>()
            .CreateServer(xmlRpcServer);
            
        //var eventReceiver = new CcuEventReceiver();
        //eventReceiver.EventMessageTopic
        //    .Register<HomeMaticEventMessage>()
        //    .SubscribeOn(new TaskPoolScheduler(new TaskFactory()))
        //    .Subscribe(msg => Console.WriteLine($"{msg.Address}.{msg.ValueKey} = {msg.Value}"));
            
        StartServer(ccuXmlRpcServer, homeMaticXmlRpcApi);
            
        Console.ReadKey();

        await homeMaticXmlRpcApi.InitAsync("", "TestIntf");

        await ccuXmlRpcServer.StopAsync();
    }

    private void StartServer(ICcuXmlRpcEventServer ccuXmlRpcServer, IHomeMaticXmlRpcApi homeMaticXmlRpcApi)
    {
        Task.Run(ccuXmlRpcServer.StartAsync);
            
        homeMaticXmlRpcApi.InitAsync(XmlRpcUrl, "TestIntf");
    }

    private string XmlRpcUrl { get; } = $"http://{new NetworkInfo().GetHostName()}:12345";
}