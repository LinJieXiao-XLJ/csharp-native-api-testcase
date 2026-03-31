// IoTDB C# Native API Test Case
// Entry point for the test application

using csharp_native_api_testcase.Tests.Tree;

namespace csharp_native_api_testcase;

class Program
{
    static async Task Main(string[] args)
    {
        // 无参数默认执行全部测试
        if (args.Length == 0)
        {
            await RunTreeTests();
            await RunTableTests();
            return;
        }

        // 解析参数
        switch (args[0].ToLower())
        {
            case "tree":
                await RunTreeTests();
                break;
            case "table":
                await RunTableTests();
                break;
            case "-h":
            case "--help":
            case "help":
                PrintUsage();
                break;
            default:
                Console.WriteLine($"Unknown argument: {args[0]}");
                PrintUsage();
                break;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run [command]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  tree           Run tree model tests");
        Console.WriteLine("  table          Run table model tests");
        Console.WriteLine("  help           Show this help message");
        Console.WriteLine();
        Console.WriteLine("Note: Running without arguments executes all tests by default.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run           # Run all tests");
        Console.WriteLine("  dotnet run -- tree   # Run tree model tests");
        Console.WriteLine("  dotnet run -- table  # Run table model tests");
    }

    static async Task RunTreeTests()
    {
        var test = new Session_pool_Test();
        await test.RunAllTests();
    }

    static async Task RunTableTests()
    {
        await Task.CompletedTask;
    }
}