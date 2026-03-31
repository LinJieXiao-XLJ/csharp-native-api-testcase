// IoTDB C# Native API Test Case
// Entry point for the test application

namespace csharp_native_api_testcase;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("IoTDB C# Native API Test Case");
        Console.WriteLine("================================");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  --table    Run table model tests");
        Console.WriteLine("  --tree     Run tree model tests");
        Console.WriteLine("  --all      Run all tests");
        Console.WriteLine();

        // TODO: Implement test runner
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided. Use --help for usage information.");
            return;
        }

        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                case "--table":
                    Console.WriteLine("Running table model tests...");
                    // RunTableTests();
                    break;
                case "--tree":
                    Console.WriteLine("Running tree model tests...");
                    // RunTreeTests();
                    break;
                case "--all":
                    Console.WriteLine("Running all tests...");
                    // RunAllTests();
                    break;
                case "--help":
                case "-h":
                    Console.WriteLine("Usage: dotnet run -- [options]");
                    Console.WriteLine("Options:");
                    Console.WriteLine("  --table    Run table model tests");
                    Console.WriteLine("  --tree     Run tree model tests");
                    Console.WriteLine("  --all      Run all tests");
                    Console.WriteLine("  --help     Show this help message");
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {arg}");
                    break;
            }
        }
    }
}