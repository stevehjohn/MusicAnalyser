using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MusicAnalyser.Core;
using Newtonsoft.Json;
using static System.Console;

namespace MusicAnalyser.Console
{
    public static class Program
    {
        public static void Main()
        {
            const string path = "C:\\Users\\Stevo\\Music\\Iron Maiden";

            Dictionary<string, ulong[]> fingerprints;

            if (! File.Exists("cache.json"))
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                WriteLine("Cache not found, generating fingerprints.");

                fingerprints = GetFingerprints(path);

                stopwatch.Stop();

                WriteLine($"{fingerprints.Count} fingerprints generated in {(int) stopwatch.Elapsed.TotalSeconds}s");

                File.WriteAllText("cache.json", JsonConvert.SerializeObject(fingerprints));
            }
            else
            {
                WriteLine("Cache found.");

                fingerprints = JsonConvert.DeserializeObject<Dictionary<string, ulong[]>>(File.ReadAllText("cache.json"));
            }

        }

        private static Dictionary<string, ulong[]> GetFingerprints(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.mp3", SearchOption.AllDirectories);

            var fingerprints = new Dictionary<string, ulong[]>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                if (fingerprints.ContainsKey(fileName)) 
                { 
                    continue;
                }

                Write($"Fingerprinting {fileName}... ");

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                var fingerprint = Fingerprinter.GetFingerprint(file);

                stopwatch.Stop();

                WriteLine($"{stopwatch.ElapsedMilliseconds:##0,000}ms");

                fingerprints.Add(Path.GetFileName(file), fingerprint);
            }

            return fingerprints;
        }
    }
}
