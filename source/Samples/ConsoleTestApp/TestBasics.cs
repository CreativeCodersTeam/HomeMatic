﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using CreativeCoders.Core;
using CreativeCoders.Di;
using CreativeCoders.Di.MsServiceProvider;
using CreativeCoders.DynamicCode.Proxying;
using CreativeCoders.HomeMatic.Api;
using CreativeCoders.HomeMatic.Core;
using CreativeCoders.HomeMatic.XmlRpc.Client;
using CreativeCoders.HomeMatic.XmlRpc.Server;
using CreativeCoders.Net;
using CreativeCoders.Net.Servers.Http.AspNetCore;
using CreativeCoders.Net.XmlRpc.Proxy;
using CreativeCoders.Net.XmlRpc.Server;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleTestApp
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class TestBasics
    {
        private IDiContainer _container;

        public async Task Run()
        {
            _container = CreateDiContainer();

            var apiBuilder = _container.GetInstance<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
            
            var xmlRpcApiHmIp = apiBuilder
                .ForUrl("http://homematic-ccu2:" + CcuRpcPorts.HomeMaticIp)
                .Build();

            var xmlRpcApi = apiBuilder
                .ForUrl("http://homematic-ccu2:" + CcuRpcPorts.HomeMatic)
                .Build();

            var connection = new CcuConnection(xmlRpcApiHmIp);

            var deviceInfos = await connection.GetDeviceInfosAsync();

            var devices = await connection.GetDevicesAsync();

            var temperature = await xmlRpcApi.GetValueAsync<double>("NEQ1142873:1", "TEMPERATURE");

            Console.WriteLine($"Temperature: {temperature}");

            var deviceDescriptions = await xmlRpcApiHmIp.ListDevicesAsync();
            
            deviceDescriptions.ForEach(x => Console.WriteLine($"{x.Address}"));
            
            Console.WriteLine();

            await LoadParameterDescriptions(xmlRpcApiHmIp);
        }

        private static IDiContainer CreateDiContainer()
        {
            var serviceCollection = new ServiceCollection() as IServiceCollection;
            var containerBuilder = new ServiceProviderDiContainerBuilder(serviceCollection);

            containerBuilder.AddTransient<IXmlRpcProxyBuilder<IHomeMaticXmlRpcApi>, XmlRpcProxyBuilder<IHomeMaticXmlRpcApi>>();
            containerBuilder.AddTransient(typeof(IProxyBuilder<>), typeof(ProxyBuilder<>));
            
            serviceCollection.AddHttpClient();

            return containerBuilder.Build();
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

            var xmlRpcServer = new XmlRpcServer(httpServer)
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };
            xmlRpcServer.Urls.Add(XmlRpcUrl);
            
            var ccuXmlRpcServer =
                new CcuXmlRpcEventServer(xmlRpcServer);
            
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

        private void StartServer(CcuXmlRpcEventServer ccuXmlRpcServer, IHomeMaticXmlRpcApi homeMaticXmlRpcApi)
        {
            Task.Run(ccuXmlRpcServer.StartAsync);
            
            homeMaticXmlRpcApi.InitAsync(XmlRpcUrl, "TestIntf");
        }

        private string XmlRpcUrl { get; } = $"http://{new NetworkInfo().GetHostName()}:12345";
    }
}