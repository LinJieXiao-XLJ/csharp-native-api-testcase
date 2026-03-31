// IoTDB C# Native API Test Case
// Entry point for the test application

using csharp_native_api_testcase.Tests.Tree;

namespace csharp_native_api_testcase;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("IoTDB C# Native API Test Case");
        Console.WriteLine("================================");
        Console.WriteLine();

        // 无参数默认执行全部测试
        if (args.Length == 0)
        {
            Console.WriteLine("Running all tests...\n");
            await RunTreeTests();
            await RunTableTests();
            return;
        }

        // 解析参数
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-m":
                    if (i + 1 < args.Length)
                    {
                        var mode = args[i + 1].ToLower();
                        i++; // 跳过下一个参数
                        switch (mode)
                        {
                            case "tree":
                                Console.WriteLine("Running Tree model tests...\n");
                                await RunTreeTests();
                                break;
                            case "table":
                                Console.WriteLine("Running Table model tests...\n");
                                await RunTableTests();
                                break;
                            default:
                                Console.WriteLine($"Unknown mode: {mode}");
                                PrintUsage();
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: -m requires a mode argument (tree/table)");
                        PrintUsage();
                    }
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {args[i]}");
                    PrintUsage();
                    break;
            }
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  (no args)      Run all tests (default)");
        Console.WriteLine("  -m tree        Run tree model tests");
        Console.WriteLine("  -m table       Run table model tests");
        Console.WriteLine("  -h, --help     Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run              # Run all tests");
        Console.WriteLine("  dotnet run -m tree      # Run tree model tests");
        Console.WriteLine("  dotnet run -m table     # Run table model tests");
    }

    static async Task RunTreeTests()
    {
        var test = new Session_pool_Test();
        await test.RunAllTests();
    }

    static async Task RunTableTests()
    {
        Console.WriteLine("Table model tests not implemented yet.");
        Console.WriteLine("Please add test files to tests/table/ directory.");
        await Task.CompletedTask;
    }
}