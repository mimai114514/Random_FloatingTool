using System;
using System.Threading;

Console.WriteLine("Simulating bad random generation (new Random() every 20ms)...");

for (int i = 0; i < 10; i++)
{
    // Simulate the bug: creating a new Random instance every time
    Random random = new Random();
    int value = random.Next(1, 10); // Generating between 1 and 9
    Console.WriteLine($"Tick {i}: Generated {value} (Time: {DateTime.Now:ss.fff})");

    // Simulate the timer interval
    Thread.Sleep(20);
}
