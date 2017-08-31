using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Maca134.Arma.ExtensionTesterConsole
{
    internal class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            string dllPath = null;
            if (args.Length == 0)
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Dll File|*.dll"
                };
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    Console.WriteLine("no dll selected");
                    Thread.Sleep(4000);
                    Environment.Exit(1);
                    return;
                }
                dllPath = openFileDialog.FileName;
            }
            else
            {
                dllPath = args[0];
            }
            ArmaDll dll;
            try
            {
                dll = new ArmaDll(dllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dll: {ex.Message}");
                Thread.Sleep(4000);
                Environment.Exit(2);
                return;
            }
            Console.WriteLine($"{dllPath} loaded");
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
                    Console.WriteLine($"Error calling dll: {ex.Message} -> {ex.InnerException?.Message}");
                }
                watch.Stop();
                Console.WriteLine(result == string.Empty ? $"{watch.ElapsedMilliseconds}ms" : $"{result} ({watch.ElapsedMilliseconds}ms)");
            }
            dll.Dispose();
        }
    }
}
