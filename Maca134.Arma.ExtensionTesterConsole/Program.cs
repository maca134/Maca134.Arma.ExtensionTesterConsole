using System;
using System.Diagnostics;

namespace Maca134.Arma.ExtensionTesterConsole
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("must call with a path to the dll");
                Environment.Exit(1);
                return;
            }
            ArmaDll dll;
            try
            {
                dll = new ArmaDll(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dll: {ex.Message}");
                Environment.Exit(2);
                return;
            }
            Console.WriteLine($"{args[0]} loaded");
            Console.WriteLine("Type 'exit' to quit");
            Console.WriteLine();

            while (true)
            {
                Console.Write("#> ");
                var cmd = Console.ReadLine() ?? "";
                if (cmd.ToLower() == "exit")
                    break;
                var watch = Stopwatch.StartNew();
                var result = string.Empty;
                try
                {
                    result = dll.Call(cmd.Replace("\\n", "\n"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling dll: {ex.Message}");
                }
                watch.Stop();
                Console.WriteLine(result == string.Empty ? $"{watch.ElapsedMilliseconds}ms" : $"{result} ({watch.ElapsedMilliseconds}ms)");
            }
            dll.Dispose();
        }
    }
}
