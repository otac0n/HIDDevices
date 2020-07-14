﻿// Licensed under the Apache License, Version 2.0 (the "License").
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HIDDevices.Sample
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var samples = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract &&
                            (t.IsValueType ||
                             (t.GetInterfaces().Contains(typeof(ISample)) &&
                              t.GetConstructor(Type.EmptyTypes) != null)))
                .Select(Activator.CreateInstance)
                .OfType<ISample>()
                .ToArray();

            ISample? sample = null;
            if (args.Length != 1 ||
                (sample = Array.Find(samples, s => s.ShortNames.Contains(args[0]))) is null)
            {
                var assemblyName = assembly.GetName().Name;
                var location = Path.GetFileName(assembly.Location) ?? assemblyName;
                // We appear to have a cry for help!
                Console.WriteLine($"{assemblyName} - Sample executor");
                Console.WriteLine();

                do
                {
                    Console.WriteLine("Please select one of the following samples to run.");

                    // Create instances of all sample classes
                    foreach (var s in samples)
                    {
                        Console.WriteLine($"  [{string.Join('|', s.ShortNames)}]");
                        Console.WriteLine($"    {s.FullName}");
                        Console.WriteLine($"    {s.Description}");
                        Console.WriteLine();
                    }

                    if (!Environment.UserInteractive) return;

                    var option = Console.ReadLine();
                    sample = Array.Find(samples, s => s.ShortNames.Contains(option));
                } while (sample is null);
            }

            Console.WriteLine($"Running {sample.FullName}...");
            Console.WriteLine();
            await sample.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
