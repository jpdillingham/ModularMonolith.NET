using System;
using System.IO;
using System.Linq;

namespace Common;

public static class Dotenv
{
    public static void Load(string path)
    {
        Console.WriteLine();

        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"🤷 Dotenv: No .env file found at {path}");
                return;
            }

            Console.WriteLine($"🔎 Dotenv: Loading environment variables from {path}...\n");

            var lines = File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'));

            if (lines.Count() < 1)
            {
                Console.WriteLine("🤷 Dotenv: No environment variables defined!");
                return;
            }

            foreach (var line in lines)
            {
                var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);

                Console.WriteLine($"🔑 Dotenv: {parts[0]}=<redacted {parts[1].Length} characters> ");
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }

            Console.WriteLine("\n🚀 Dotenv: Environment variables loaded");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("\n❌ Dotenv: Failed");
        }
        finally
        {
            Console.WriteLine();
        }
    }
}
