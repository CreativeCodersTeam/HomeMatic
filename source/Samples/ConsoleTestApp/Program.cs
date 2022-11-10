using System;
using System.Threading.Tasks;

namespace ConsoleTestApp;

public static class Program
{
    public static async Task Main()
    {
        // ConsoleLog.Init();
        // ConsoleLog.IsDebugEnabled = true;
            
        await new TestBasics().Run();
            
        Console.WriteLine("Press key to exit...");
        Console.ReadKey();
    }
}