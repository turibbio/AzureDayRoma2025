using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace SemanticKernelOrchestration;

public class Program
{
    private static Kernel? _kernel;
    private static InProcessRuntime? _runtime;
    private static bool _isLoading = true;

    public static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        DotNetEnv.Env.Load();

        // Initialize the kernel and runtime
        InitializeKernel();
        _runtime = new InProcessRuntime();
        await _runtime.StartAsync();

        // Main application loop
        bool isRunning = true;
        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("=== Semantic Kernel Orchestration Demo ===");
            Console.WriteLine("1. Run Concurrent Orchestration (Physics & Chemistry Experts)");
            Console.WriteLine("2. Run Sequential Orchestration (Marketing Pipeline)");
            Console.WriteLine("3. Run Group Chat Orchestration (Copywriter & Reviewer)");
            Console.WriteLine("4. Run Handoff Orchestration (Customer Support)");
            Console.WriteLine("5. Exit");
            Console.Write("\nSelect an option: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await RunConcurrentOrchestration();
                    break;
                case "2":
                    await RunSequentialOrchestration();
                    break;
                case "3":
                    await RunGroupChatOrchestration();
                    break;
                case "4":
                    await RunHandoffOrchestration();
                    break;
                case "5":
                    isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }

        await _runtime.StopAsync();
    }

    private static void InitializeKernel()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        // Get environment variables with default fallbacks
        string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";
        string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "your-api-key-here";
        string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-endpoint-here.openai.azure.com/";

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            apiKey: apiKey,
            endpoint: endpoint
        );

        kernelBuilder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
        _kernel = kernelBuilder.Build();
    }

    private static async Task RunConcurrentOrchestration()
    {
        Console.WriteLine("=== Running Concurrent Orchestration ===");
        Console.Write("Enter your question (e.g., What is temperature?): ");
        string? prompt = Console.ReadLine() ?? "What is temperature?";            Console.WriteLine("\nProcessing with both Physics and Chemistry experts...\n");
            
            // Start a loading animation in a separate task
            var loadingTask = Task.Run(() => ShowLoadingAnimation());

            if (_kernel != null && _runtime != null)
            {
                var concurrentDemo = new ConcurrentOrchestrationDemo(_kernel);
                string result = await concurrentDemo.RunOrchestration(prompt, _runtime);
                
                // Stop the loading animation
                _isLoading = false;
                await loadingTask;

            Console.WriteLine($"\n{result}");
        }
        else
        {
            Console.WriteLine("Error: Kernel or runtime not initialized correctly.");
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static async Task RunSequentialOrchestration()
    {
        Console.WriteLine("=== Running Sequential Orchestration ===");
        Console.Write("Enter a product description to create marketing copy: ");
        string? prompt = Console.ReadLine() ?? "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hours";            Console.WriteLine("\nProcessing through the marketing pipeline...\n");
            
            // Start a loading animation in a separate task
            _isLoading = true;
            var loadingTask = Task.Run(() => ShowLoadingAnimation());

            if (_kernel != null && _runtime != null)
            {
                var sequentialDemo = new SequentialOrchestrationDemo(_kernel);
                var (result, history) = await sequentialDemo.RunOrchestration(prompt, _runtime);
                
                // Stop the loading animation
                _isLoading = false;
                await loadingTask;

            Console.WriteLine($"\nFinal Marketing Copy:\n{result}\n");
            Console.WriteLine("Orchestration Process History:");
            foreach (var message in history)
            {
                WriteAgentChatMessage(message);
            }
        }
        else
        {
            Console.WriteLine("Error: Kernel or runtime not initialized correctly.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static async Task RunGroupChatOrchestration()
    {
        Console.WriteLine("=== Running Group Chat Orchestration ===");
        Console.Write("Enter a product to create a marketing slogan for: ");
        string? prompt = Console.ReadLine() ?? "An electric SUV that is affordable and fun to drive";
        Console.WriteLine("\nProcessing with Copywriter and Reviewer agents...\n");
            
        // Start a loading animation in a separate task
        _isLoading = true;
        var loadingTask = Task.Run(() => ShowLoadingAnimation());

        if (_kernel != null && _runtime != null)
        {
            var groupChatDemo = new GroupChatOrchestrationDemo(_kernel);
            var (result, history) = await groupChatDemo.RunOrchestration(prompt, _runtime);
                
            // Stop the loading animation
            _isLoading = false;
            await loadingTask;

            Console.WriteLine($"\nFinal Slogan:\n{result}\n");
            Console.WriteLine("Group Chat Orchestration Process History:");
            foreach (var message in history)
            {
                WriteAgentChatMessage(message);
            }
        }
        else
        {
            Console.WriteLine("Error: Kernel or runtime not initialized correctly.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static async Task RunHandoffOrchestration()
    {
        Console.WriteLine("=== Running Handoff Orchestration ===");
        Console.WriteLine("This demo simulates a customer support system with agent handoffs.");
        Console.WriteLine("The system will automatically process a series of customer interactions.\n");
            
        // Start a loading animation in a separate task
        _isLoading = true;
        var loadingTask = Task.Run(() => ShowLoadingAnimation());

        if (_kernel != null && _runtime != null)
        {
            var handoffDemo = new HandoffOrchestrationDemo(_kernel);
            var (result, history) = await handoffDemo.RunOrchestration("I am a customer that needs help with my orders", _runtime);
                
            // Stop the loading animation
            _isLoading = false;
            await loadingTask;

            Console.WriteLine($"\nFinal Result:\n{result}\n");
            Console.WriteLine("Handoff Orchestration Process History:");
            foreach (var message in history)
            {
                WriteAgentChatMessage(message);
            }
        }
        else
        {
            Console.WriteLine("Error: Kernel or runtime not initialized correctly.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static void WriteAgentChatMessage(ChatMessageContent message)
    {
        var originalColor = Console.ForegroundColor;
        
        if (message.Role == AuthorRole.Assistant)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            // Access metadata and get agent name if available
            string agentName = "Assistant";
            if (message.Metadata != null && message.Metadata.TryGetValue("agent", out var agent))
            {
                agentName = agent?.ToString() ?? "Assistant";
            }
            Console.WriteLine($"\n[{agentName}]: {message.Content}");
        }
        else if (message.Role == AuthorRole.User)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\n[User]: {message.Content}");
        }
        else if (message.Role == AuthorRole.System)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[System]: {message.Content}");
        }
        else
        {
            Console.WriteLine($"\n[{message.Role}]: {message.Content}");
        }
        
        Console.ForegroundColor = originalColor;
    }

    private static async Task ShowLoadingAnimation()
    {
        string[] animationFrames = { "⠋", "⠙", "⠸", "⠴", "⠦", "⠇" };
        int frameIndex = 0;
        int cursorLeft = Console.CursorLeft;
        int cursorTop = Console.CursorTop;
        
        while (_isLoading)
        {
            Console.SetCursorPosition(0, cursorTop);
            Console.Write($"{animationFrames[frameIndex]} Working on it...");
            frameIndex = (frameIndex + 1) % animationFrames.Length;
            await Task.Delay(100);
        }
        
        Console.SetCursorPosition(0, cursorTop);
        Console.Write(new string(' ', 20)); // Clear the loading text
        Console.SetCursorPosition(0, cursorTop);
    }
}
