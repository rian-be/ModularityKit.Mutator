using BillingQuotas.Policies;
using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Mutator.Abstractions;
using ModularityKit.Mutator.Abstractions.Engine;

namespace BillingQuotas;

internal static class Program
{
    private static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddMutators(MutationEngineOptions.Strict, addDefaultLoggingInterceptor: true);
       
        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IMutationEngine>();
       
        engine.RegisterPolicy(new MaxQuotaPolicy());
        engine.RegisterPolicy(new PreventNegativeQuotaPolicy());
        
        Console.WriteLine("=== ModularityKit.Mutators - Complete Example ===\n");
        
        await Scenarios.EmergencyIncreaseScenario.Run(engine);
        await Scenarios.MonthlyResetScenario.Run(engine);
        Console.WriteLine("\n METRICS & STATISTICS");

        var stats = await engine.GetStatisticsAsync();

        Console.WriteLine($"\n Mutation Statistics:");
        Console.WriteLine($"  Total executed: {stats.TotalExecuted}");

        Console.WriteLine($"\n Performance Metrics:");
        Console.WriteLine($"  Average execution time: {stats.AverageExecutionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Median execution time: {stats.MedianExecutionTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  P95 execution time: {stats.P95ExecutionTime.TotalMilliseconds:F2} ms");
    }
}