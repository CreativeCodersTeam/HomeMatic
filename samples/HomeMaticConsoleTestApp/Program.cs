// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using CreativeCoders.HomeMatic.JsonRpc;
using CreativeCoders.HomeMatic.JsonRpc.Api;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddJsonRpcClient();

var sp = services.BuildServiceProvider();

var apiBuilder = sp.GetRequiredService<IHomeMaticJsonRpcApiBuilder>();

var api = apiBuilder.ForUrl(new Uri("http://192.168.2.210/api/homematic.cgi")).Build();

var password = Console.ReadLine();

var loginResponse = await api.LoginAsync("Admin", password);

Console.WriteLine(JsonSerializer.Serialize(loginResponse));

var doLoginResponse = await api.LoginAsync("Admin", password);
        
Console.WriteLine($"DoLogin with api builder Response: {doLoginResponse}");

Console.WriteLine("Hello, World!");
